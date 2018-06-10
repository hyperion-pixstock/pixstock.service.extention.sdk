using Pixstock.Service.Infra.Model;

namespace Pixstock.Nc.Srv.Ext
{
    public class DefaultCutpoint : IInitPluginCutpoint, IStartCutpoint, ICategoryApiCutpoint, ICreateCategoryCutpoint
    {
        public void OnInitPlugin(object param)
        {

        }

        public void OnCreateCategory(ICategory category)
        {

        }

        public void OnGetCategory(ICategory category)
        {

        }

        public void Process(object param)
        {

        }
    }
}