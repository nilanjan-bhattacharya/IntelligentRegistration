using System.IO;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace EmergencyServicesBot.Infra
{
    public static class Utility
    {
        public static async Task<string> GetCardText(string cardName)
        {
            var path = HostingEnvironment.MapPath($"/Cards/{cardName}.json");
            if (!File.Exists(path))
                return string.Empty;

            using (var f = File.OpenText(path))
            {
                return await f.ReadToEndAsync();
            }
        }
    }
}