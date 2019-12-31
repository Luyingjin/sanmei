using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanmei_AirConditioner.Helpers
{
    public class Base64Helper
    {
        public static string EncodeBase64(string code)
        {
            try
            {
                return Convert.ToBase64String(Encoding.UTF8.GetBytes(code));
            }
            catch
            {
                return code;
            }
        }

        public static string DecodeBase64(string code)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(code);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return code;
            }
        }
    }
}
