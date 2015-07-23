using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoFacWrapper;
using Caliburn.Micro;
using SqliteCompare.Entity.Annotations;
using SqliteCompare.Views;

namespace SqliteCompare.Shell
{
   public  class ShellViewModel : Conductor<IScreen>.Collection.OneActive, IScreen
    {
       public ShellViewModel()
       {
            ActivateItem(ClassFactory.GetInstance<ICompareViewModel>());
       }
    }
}
