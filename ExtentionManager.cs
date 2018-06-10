using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog;
using Pixstock.Service.Infra.Extention;
using Pixstock.Service.Infra.Model;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Lifestyles;

namespace Pixstock.Nc.Srv.Ext
{
    /// <summary>
    /// 拡張機能の読み込みや操作するクラス
    /// </summary>
    public class ExtentionManager
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

        public readonly Container container;

        private readonly Container extention_container;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="container">DIコンテナ</param>
        public ExtentionManager(Container container)
        {
            this.container = container;
            this.extention_container = new Container();

            container.Register<IExtentionRunner, ExtentionRunner>(Lifestyle.Singleton);
        }

        /// <summary>
        /// ディレクトリから拡張機能ファイルを読み込み、拡張機能を追加する
        /// </summary>
        /// <param name="pluginDirectory">拡張機能ファイルが格納されているディレクトリパス</param>
        public void InitializePlugin(string pluginDirectory)
        {
            try
            {
                var pluginAssemblies =
                    from file in new DirectoryInfo(pluginDirectory).GetFiles()
                    where file.Extension.ToLower() == ".pex"
                    select Assembly.LoadFile(file.FullName);
                if (pluginAssemblies.Count() > 0)
                {
                    _logger.Info("拡張機能({0})の読み込みを開始します。", pluginDirectory);
                    extention_container.RegisterCollection<IExtentionMetaInfo>(pluginAssemblies);
                }
            }
            catch (IOException e)
            {
                _logger.Warn(e);
                _logger.Warn("拡張機能の読み込みに失敗しました。 pluginDirectory={0}", pluginDirectory);
            }
        }

        /// <summary>
        /// 拡張機能を追加する
        /// </summary>
        /// <param name="pluginClazz">拡張機能のクラス情報</param>
        public void AddPlugin(Type pluginClazz)
        {
            extention_container.Collections.AppendTo(typeof(IExtentionMetaInfo), pluginClazz);
        }

        /// <summary>
        /// 拡張機能の初期設定を完了する
        /// </summary>
        /// <remarks>
        /// 拡張機能の初期設定を完了し、拡張機能およびカットポイントを呼び出せる状態にします。
        /// このメソッドは一度だけ呼び出します。
        /// </remarks>
        public void CompletePlugin()
        {
            // 拡張機能からカットポイント別インターフェース実装クラスを登録する

            extention_container.Verify();

            // DIコンテナに登録したインターフェースを実装した実態が1つも存在しない場合、
            // DIコンテナからのリゾルブ時に例外が発生するため、DefaultCutpointにはすべてのインターフェースを実装します。
            RegisterCutpoint(typeof(DefaultCutpoint));

            try
            {
                var it = extention_container.GetAllInstances<IExtentionMetaInfo>();
                foreach (var prop in it)
                {
                    container.Register(prop.GetType()); // 拡張機能のメタ情報オブジェクトそのものもDIで管理する

                    foreach (var cutpoint in prop.Cutpoints())
                    {
                        RegisterCutpoint(cutpoint);
                    }

                }
            }
            catch (SimpleInjector.ActivationException)
            {
                _logger.Warn("拡張機能のインターフェース取得に失敗しました。");
            }
        }

        /// <summary>
        /// カットポイントの処理を実行する
        /// </summary>
        /// <param name="cutpoint">実行したいカットポイント</param>
        /// <param name="param">任意のパラメータ</param>
        public void Execute(ExtentionCutpointType cutpoint, object param = null)
        {
            switch (cutpoint)
            {
                case ExtentionCutpointType.INIT:
                    Execute_INIT(param);
                    break;
                case ExtentionCutpointType.START:
                    Execute_START(param);
                    break;
                case ExtentionCutpointType.API_GET_CATEGORY:
                    Execute_API_GET_CATEGORY(param);
                    break;
            }
        }

        private void Execute_INIT(object param)
        {
            var ite = container.GetAllInstances<IInitPluginCutpoint>();
            foreach (var prop in ite)
            {
                try
                {
                    prop.OnInitPlugin(param);
                }
                catch (Exception expr)
                {
                    _logger.Warn("拡張機能の実行でエラーは発生しました。");
                    _logger.Warn(expr);
                }
            }
        }

        private void Execute_START(object param)
        {
            var ite = container.GetAllInstances<IStartCutpoint>();
            foreach (var prop in ite)
            {
                try
                {
                    prop.Process(param);
                }
                catch (Exception expr)
                {
                    _logger.Warn("拡張機能の実行でエラーは発生しました。");
                    _logger.Warn(expr);
                }
            }
        }

        private void Execute_API_GET_CATEGORY(object param)
        {
            var ite = container.GetAllInstances<ICategoryApiCutpoint>();
            foreach (var prop in ite.Select(p => (ICategoryApiCutpoint)p))
            {

                try
                {
                    prop.OnGetCategory((ICategory)param);
                }
                catch (Exception expr)
                {
                    _logger.Warn("拡張機能の実行でエラーは発生しました。");
                    _logger.Warn(expr);
                }
            }
        }

        private void RegisterCutpoint(Type cutpoint)
        {
            // 各カットポイントのインターフェースを、DIコンテナに登録する。
            if ((typeof(IInitPluginCutpoint)).IsAssignableFrom(cutpoint))
            {
                container.Collections.AppendTo(typeof(IInitPluginCutpoint), cutpoint);
            }

            if ((typeof(IStartCutpoint)).IsAssignableFrom(cutpoint))
            {
                container.Collections.AppendTo(typeof(IStartCutpoint), cutpoint);
            }

            if ((typeof(ICategoryApiCutpoint)).IsAssignableFrom(cutpoint))
            {
                container.Collections.AppendTo(typeof(ICategoryApiCutpoint), cutpoint);
            }
        }

    }
}