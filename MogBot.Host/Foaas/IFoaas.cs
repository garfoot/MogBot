using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace MogBot.Host.Foaas
{
    public interface IFoaas
    {
        [Get("/operations")]
        Task<IList<FoaasOperation>> GetOperations();
    }
}