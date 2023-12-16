using System.Text;

namespace ProductsWebApiPD
{
    public class KeyProvider
    {
        private static string keyString = "super_secret_string_123_poiuytrewqasdfghjklzxcvbnm";
        public static byte[] Key => Encoding.Default.GetBytes(keyString);
    }
}
