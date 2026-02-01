namespace CY_WebApi.Models
{
    public class AccountSnDb
    {


        //public static bool IsSNDb3 = true;

        public static bool IsSNDb3 = false;


        public static int MojodiKala => IsSNDb3 ? 24 : 7;
        public static int BahayKala => IsSNDb3 ? 14 : 17;
        public static int DarAmad => IsSNDb3 ? 15 : 15;
        public static int bargashtAzFrosh => IsSNDb3 ? 146 : 33;
    }
}
