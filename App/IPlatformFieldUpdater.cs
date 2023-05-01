using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    internal interface IPlatformFieldUpdater
    {
        public string personalAccessToken { get; init; }
        public IBindingPart BindingPart { get; init; }
        public Task UpdateCompletedTimeAsync(double completed);
    }
}
