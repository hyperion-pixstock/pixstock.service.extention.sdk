namespace Pixstock.Nc.Srv.Ext
{
    public interface IInitPluginCutpoint : ICutpoint
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        void OnInitPlugin(object param);
    }
}