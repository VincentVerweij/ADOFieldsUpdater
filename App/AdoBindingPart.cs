using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    internal class AdoBindingPart : IBindingPart
    {
        private AdoBindingPart()
        {

        }

        public AdoBindingPart(string connectionNameToBind, int connectionItemIdToUpdate)
        {
            ConnectionNameToBind = connectionNameToBind;
            ConnectionItemIdToUpdate = connectionItemIdToUpdate;
        }

        public static string BindingPlaceholder => "ADO";
        public string ConnectionNameToBind { get; init; }
        public int ConnectionItemIdToUpdate { get; init; }

    }
}
