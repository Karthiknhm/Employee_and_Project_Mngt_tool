using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Employee_Info.Models;

namespace Employee_Info.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserRoleContext _dbContext = new UserRoleContext();
        SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=EmpDB;Integrated Security=True");

        public JsonResult GetRoleData()
        {
            sqlCon.Open();

            SqlCommand SP_M_Role = new SqlCommand("SP_M_Role", sqlCon);
            SP_M_Role.CommandType = CommandType.StoredProcedure;
            SqlDataReader dataReader = SP_M_Role.ExecuteReader();

            List<Role> roles = new List<Role>();

            while (dataReader.Read())
            {
                Role role = new Role();

                role.RoleId = Convert.ToInt32(dataReader["RoleId"]);
                role.RoleName = dataReader["RoleName"].ToString();
                roles.Add(role);
            }

            sqlCon.Close();

            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Login(Tb_M_User user)
        {
            string strmsg = "", strurl = "";
            try
            {
                bool IsValidUser = _dbContext.Tb_M_User
                .Any(u => u.UserName.ToLower() == user
                .UserName.ToLower() && user
                .Uspassword == user.Uspassword);

                if (IsValidUser)
                {
                    var nRoleId = _dbContext.Tb_M_User.Where(u => u.UserName == user.UserName).SingleOrDefault().UsRoleId;
                    string userRoleName = _dbContext.Tb_M_Role.Where(u => u.RoleId == nRoleId).SingleOrDefault().RoleName;
                    FormsAuthentication.SetAuthCookie(user.UserName, false);
                    strmsg = "Validuser";
                    if (userRoleName == "Admin")
                    {
                        strurl = Url.Action("", "Employee/Details");
                    }
                    else if(userRoleName == "Manager")
                    {
                        strurl = Url.Action("", "Project/CreateProject");
                    }
                }
                else
                {   
                    strmsg = "invalid Username and password";
                    strurl = Url.Action("", "");
                }

            }
            catch (Exception e)
            {
                ViewBag.msg = e.Message;
                return Json(new
                {
                    msg = e.Message,
                    url = strurl
                }, JsonRequestBehavior.AllowGet);
            }        
            return Json(new
            {
                msg = strmsg,
                url = strurl
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(Tb_M_User registerUser)
            {
            string strmsg = "", strurl = "";
            try
            {
                bool bIsExist = _dbContext.Tb_M_User.Any(u => u.UserName == registerUser.UserName);
                if (bIsExist)
                {
                    strmsg = "User Name already exist";
                    strurl = Url.Action("", "");
                    return Json(new
                    {
                        msg = strmsg,
                        url = strurl
                    }, JsonRequestBehavior.AllowGet);
                }
                _dbContext.Tb_M_User.Add(registerUser);
                int result  = _dbContext.SaveChanges();
                if (result > 0)
                {
                    strmsg = "User Added Successfully";
                    strurl = Url.Action("", "Account/Login");
                }
                else
                {
                    strmsg = "User not added Successfully";
                    strurl = Url.Action("", "");
                }
            }
            catch (Exception e)
            {
                ViewBag.msg = e.Message;
            }
            return Json(new
            {
                msg = strmsg,
                url = strurl
            }, JsonRequestBehavior.AllowGet);

            return View();
        }
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            //Session.Abandon();

            //// clear authentication cookie
            //HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            //cookie1.Expires = DateTime.Now.AddYears(-1);
            //Response.Cookies.Add(cookie1);

            FormsAuthentication.RedirectToLoginPage();
            return RedirectToAction("Login");
        }
    }
}