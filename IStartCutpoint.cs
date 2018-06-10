namespace Pixstock.Nc.Srv.Ext
{
    public interface IStartCutpoint : ICutpoint
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        void Process(object param);
    }
}