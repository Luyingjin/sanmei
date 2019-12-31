using Abp.Dependency;
using System.Threading.Tasks;

namespace Sanmei_AirConditioner.MpAccountTokenContainer
{
    public interface IAccessTokenContainer : ITransientDependency
    {
        Task<string> TryGetAccessTokenAsync(string appId, string appSecret, bool getNewToken = false);
        Task RegisterAsync(string appId, string appSecret, string name = null);
    }
}
