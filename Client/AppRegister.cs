using AutoFacWrapper;
using SqliteCompare.Repository;
using SqliteCompare.Service;
using SqliteCompare.Service.InterFace;
using SqliteCompare.Views;

namespace SqliteCompare.Shell
{
    public static class AppRegister
    {
        public static void RegisterModular()
        {
            ClassFactory.RegisterTypePerDependency<SourceRepository, ISourceRepository>();
            ClassFactory.RegisterTypePerDependency<TargetRepository, ITargetRepository>();
            ClassFactory.RegisterTypeSingleInstance<CompareService, ICompareService>();
            ClassFactory.RegisterTypePerDependency<CompareViewModel, ICompareViewModel>();
        }
    }
}