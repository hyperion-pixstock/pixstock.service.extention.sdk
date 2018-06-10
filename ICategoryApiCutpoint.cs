using Pixstock.Service.Infra.Model;

namespace Pixstock.Nc.Srv.Ext
{
    /// <summary>
    /// CategoryAPIの各処理を契機として呼び出される拡張機能のカットポイントインターフェースを定義する。
    /// </summary>
    public interface ICategoryApiCutpoint : ICutpoint
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="category"></param>
         void OnGetCategory(ICategory category);
    }
}