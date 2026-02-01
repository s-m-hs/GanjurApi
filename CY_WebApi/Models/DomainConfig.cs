namespace CY_WebApi.Models
{
    public class DomainConfig
    {
        //public static bool IsDomainEnabled = false;
        public static bool IsDomainEnabled = true;


        public static string Domain => IsDomainEnabled ? ".sanecomputer.com" : null;






    }
}
