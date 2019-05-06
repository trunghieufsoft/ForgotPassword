using System.Web.Mvc;
using WM.Register.DAO;
using WM.Register.Models;

namespace WM.Register.Controllers
{
    public class Transaction_MerchantController : Controller
    {
        // GET: Transaction_Merchant
        public ActionResult TransactionList(srv_VNOGateway_transactions srv_VNOGateway_transactions)
        {
            //var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (User.Identity.IsAuthenticated)
            {
                srv_VNOGateway_transactions.merchant_id = (User as WebIPrincipal).User.id;
                var list = StoreProceduce.ListTransactionByMerchantId(srv_VNOGateway_transactions);
                return View(list);
            }
            else
            {
                return RedirectToAction("Login","Login");
            }
            
        }
    }
}