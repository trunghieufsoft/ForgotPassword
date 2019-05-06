using System.Data.Entity;

namespace WM.Register.Models
{
    public class RegisterDbContext : DbContext
    {
        public RegisterDbContext() : base("name = RegisterDbContext")
        {
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
        //public virtual DbSet<srv_VNOGateWay_Merchant> mc_merchantss { get; set; }
    }
}