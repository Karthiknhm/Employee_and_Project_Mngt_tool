using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Employee_Info.Models;

namespace Employee_Info.Controllers
{
    public class EmployeeController : Controller
    {
        SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=EmpDB;Integrated Security=True");
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult About()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Employee")]
        [Route("Details")]
        public ActionResult Details()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Form(QueryForm queryForm)
        {
            return Json(new { msg = "Thank you, our team will contact you through "+ queryForm.Email});
        }
        // GET: Emp
        public JsonResult GetEmployeeData(int nEmpID)
        {
            sqlCon.Open();

            SqlCommand SP_M_SelectTech = new SqlCommand("SP_M_SelectTech", sqlCon);
            SP_M_SelectTech.CommandType = CommandType.StoredProcedure;
            SqlDataReader dataReadeTech = SP_M_SelectTech.ExecuteReader();

            List<Technology> technologies = new List<Technology>();
            while (dataReadeTech.Read())
            {
                Technology technology = new Technology();

                technology.TechId = Convert.ToInt32(dataReadeTech["Id"]);
                technology.TechName = dataReadeTech["TechName"].ToString();
                technologies.Add(technology);
            }
            dataReadeTech.Close();

            SqlCommand SP_M_SelectBranch = new SqlCommand("SP_M_SelectBranch", sqlCon);
            SP_M_SelectBranch.CommandType = CommandType.StoredProcedure;
            SqlDataReader dataReaderBranch = SP_M_SelectBranch.ExecuteReader();

            List<Branch> branches = new List<Branch>();
            while (dataReaderBranch.Read())
            {
                Branch branch = new Branch();

                branch.Id = Convert.ToInt32(dataReaderBranch["Id"]);
                branch.Name = dataReaderBranch["BranchName"].ToString();
                branches.Add(branch);
            }
            dataReaderBranch.Close();

            SqlCommand SelecttEmployeeDetail = new SqlCommand("select * from Tb_M_Emp where Id = " + nEmpID, sqlCon);
            SqlDataReader dataReader = SelecttEmployeeDetail.ExecuteReader();

            Employee employee = new Employee();
            while (dataReader.Read())
            {

                employee.Id = Convert.ToInt32(dataReader["Id"]);
                employee.Name = dataReader["EmpName"].ToString();
                employee.Gender = dataReader["Gender"].ToString();
                employee.DOB = Convert.ToDateTime(dataReader["DOB"].ToString());
                string[] strMonth = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                employee.StrDOB = employee.DOB.Day + " " + strMonth[employee.DOB.Month - 1] + " " + employee.DOB.Year;
                employee.Email = dataReader["Email"].ToString();
                employee.Address = dataReader["EmpAddress"].ToString();

                var strTechCode = dataReader["Tech"].ToString();
                string[] CharTechCode = strTechCode.Split(',');
                string techName = "";

                for (int index = 0; index < CharTechCode.Length; index++)
                {
                    if (CharTechCode[index] != "")
                    {
                        foreach (var tech in technologies)
                        {
                            if (tech.TechId == Convert.ToUInt32(CharTechCode[index]))
                            {
                                techName += tech.TechName + ",";
                            }
                        }
                    }
                }
                if (techName.Length > 0)
                    employee.TechId = techName.Substring(0, techName.Length - 1);

               
                var CompanyBranchCode = dataReader["Branch"].ToString();
                foreach (var branch in branches)
                {
                    if (Convert.ToInt32(CompanyBranchCode) == branch.Id)
                        employee.CompanyBranchCode = branch.Name;
                }

                if (Convert.ToBoolean(dataReader["Marital"].ToString()))
                    employee.MaritalStatus = "Married";
                else
                    employee.MaritalStatus = "Single";
                employee.ActiveStatus = dataReader["ActiveStatus"].ToString();
            }

            sqlCon.Close();

            return Json(new {data = employee }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetBranchData()
        {
            sqlCon.Open();

            SqlCommand SP_M_SelectBranch = new SqlCommand("SP_M_SelectBranch", sqlCon);
            SP_M_SelectBranch.CommandType = CommandType.StoredProcedure;
            SqlDataReader dataReader = SP_M_SelectBranch.ExecuteReader();

            List<Branch> branches = new List<Branch>();

            while (dataReader.Read())
            {
                Branch branch = new Branch();

                branch.Id = Convert.ToInt32(dataReader["Id"]);
                branch.Name = dataReader["BranchName"].ToString();
                branches.Add(branch);
            }

            sqlCon.Close();

            return Json(branches, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetTechologyData()
        {
            sqlCon.Open();

            SqlCommand SP_M_SelectTech = new SqlCommand("SP_M_SelectTech", sqlCon);
            SP_M_SelectTech.CommandType = CommandType.StoredProcedure;
            SqlDataReader dataReader = SP_M_SelectTech.ExecuteReader();

            List<Technology> technologies = new List<Technology>();

            while (dataReader.Read())
            {
                Technology technology = new Technology();

                technology.TechId = Convert.ToInt32(dataReader["Id"]);
                technology.TechName = dataReader["TechName"].ToString();
                technologies.Add(technology);
            }

            sqlCon.Close();

            return Json(technologies, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllEmployee()
        {
            sqlCon.Open();

            SqlCommand SP_M_SelectBranch = new SqlCommand("SP_M_SelectBranch", sqlCon);
            SP_M_SelectBranch.CommandType = CommandType.StoredProcedure;
            SqlDataReader dataReader = SP_M_SelectBranch.ExecuteReader();

            List<Branch> branches = new List<Branch>();

            while (dataReader.Read())
            {
                Branch branch = new Branch();

                branch.Id = Convert.ToInt32(dataReader["Id"]);
                branch.Name = dataReader["BranchName"].ToString();
                branches.Add(branch);
            }

            sqlCon.Close();

            sqlCon.Open();

            SqlCommand SP_M_SelectTech = new SqlCommand("SP_M_SelectTech", sqlCon);
            SP_M_SelectTech.CommandType = CommandType.StoredProcedure;
            SqlDataReader dataReaderTech = SP_M_SelectTech.ExecuteReader();

            List<Technology> technologies = new List<Technology>();

            while (dataReaderTech.Read())
            {
                Technology technology = new Technology();

                technology.TechId = Convert.ToInt32(dataReaderTech["Id"]);
                technology.TechName = dataReaderTech["TechName"].ToString();
                technologies.Add(technology);
            }

            sqlCon.Close();


            List<Employee> lsEmp = new List<Employee> { };

            sqlCon.Open();

            SqlCommand SP_M_SelectAllUser = new SqlCommand("SP_M_SelectAllUser", sqlCon);
            SP_M_SelectAllUser.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(SP_M_SelectAllUser);
            DataSet Ds = new DataSet();
            sqlDataAdapter.Fill(Ds);

            for (int i = 0; i < Ds.Tables[0].Rows.Count; i++)
            {
                Employee cobj = new Employee();
                cobj.Id = Convert.ToInt32(Ds.Tables[0].Rows[i]["Id"].ToString());
                cobj.Name = Ds.Tables[0].Rows[i]["EmpName"].ToString();

                var GenderCode = Ds.Tables[0].Rows[i]["Gender"].ToString();
                if (GenderCode == "M")
                    cobj.Gender = "Male";
                else if (GenderCode == "F")
                    cobj.Gender = "Female";
                cobj.DOB = Convert.ToDateTime(Ds.Tables[0].Rows[i]["DOB"].ToString());
                string[] strMonth = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                cobj.StrDOB = cobj.DOB.Day + " " + strMonth[cobj.DOB.Month - 1] + " " + cobj.DOB.Year;
                cobj.Email = Ds.Tables[0].Rows[i]["Email"].ToString();
                cobj.Address = Ds.Tables[0].Rows[i]["EmpAddress"].ToString();

                var strTechCode = Ds.Tables[0].Rows[i]["Tech"].ToString();
                string[] CharTechCode = strTechCode.Split(',');
                string techName = "";
                for (int index = 0; index < CharTechCode.Length; index++)
                {
                    if (CharTechCode[index] != "")
                    {
                        foreach (var tech in technologies)
                        {
                            if (tech.TechId == Convert.ToUInt32(CharTechCode[index]))
                            {
                                techName += tech.TechName + ",";
                            }
                        }
                    }
                }
                if (techName.Length > 0)
                    cobj.TechId = techName.Substring(0, techName.Length - 1);

                var CompanyBranchCode = Ds.Tables[0].Rows[i]["Branch"].ToString();
                foreach (var branch in branches)
                {
                    if (Convert.ToInt32(CompanyBranchCode) == branch.Id)
                        cobj.CompanyBranchCode = branch.Name;
                }

                if (Convert.ToBoolean(Ds.Tables[0].Rows[i]["Marital"].ToString()))
                    cobj.MaritalStatus = "Married";
                else
                    cobj.MaritalStatus = "Single";
                cobj.ActiveStatus = Ds.Tables[0].Rows[i]["ActiveStatus"].ToString();
                cobj.isDeleted = Convert.ToBoolean(Ds.Tables[0].Rows[i]["EmpDelete"].ToString());
                if (!cobj.isDeleted)
                    lsEmp.Add(cobj);
            }
            sqlCon.Close();

            return Json(new { msg = lsEmp }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Employee employee)
        {
            sqlCon.Open();

            SqlCommand SP_M_InsertUser = new SqlCommand("SP_M_InsertUser", sqlCon);
            SP_M_InsertUser.CommandType = CommandType.StoredProcedure;

            SP_M_InsertUser.Parameters.AddWithValue("@EmpName", employee.Name);
            SP_M_InsertUser.Parameters.AddWithValue("@Gender", employee.Gender);
            SP_M_InsertUser.Parameters.AddWithValue("@DOB", employee.DOB);
            SP_M_InsertUser.Parameters.AddWithValue("@Email", employee.Email);
            SP_M_InsertUser.Parameters.AddWithValue("@EmpAddress", employee.Address);
            SP_M_InsertUser.Parameters.AddWithValue("@Tech", employee.TechId);
            SP_M_InsertUser.Parameters.AddWithValue("@Branch", employee.CompanyBranchCode);
            SP_M_InsertUser.Parameters.AddWithValue("@Marital", employee.MaritalStatus);
            SP_M_InsertUser.Parameters.AddWithValue("@EmpDelete", false);
            SP_M_InsertUser.Parameters.AddWithValue("@ActiveStatus", "Active");

            SqlDataReader sqlData = SP_M_InsertUser.ExecuteReader();
            string strresult = "";
            while (sqlData.Read())
            {
                strresult = sqlData["result"].ToString();
            }

            sqlCon.Close();

            string strPassResult;
            if (strresult == "AlreadyExist")
            {
                strPassResult = "This " + employee.Email + " user already Exist";
            }
            else
            {
                IntiateUserInPrajectMapping(employee.Email);
                strPassResult = "This " + employee.Email + " user added succefully";
            }
            return Json(new
            {
                msg = strPassResult
            });
        }
        public void IntiateUserInPrajectMapping(string strEmail)
        {
            sqlCon.Open();

            SqlCommand select_emp_id = new SqlCommand("select Id from Tb_M_Emp where Email = '" + strEmail + "'", sqlCon);

            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(select_emp_id);
            DataSet Ds = new DataSet();
            sqlDataAdapter.Fill(Ds);

            int empid = Convert.ToInt32(Ds.Tables[0].Rows[0]["Id"]);

            SqlCommand SP_M_ProjectMapping = new SqlCommand("SP_M_ProjectMapping", sqlCon);
            SP_M_ProjectMapping.CommandType = CommandType.StoredProcedure;

            SP_M_ProjectMapping.Parameters.AddWithValue("@Id", empid);
            sqlCon.Close();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Edit(Employee employee)
        {
            sqlCon.Open();

            SqlCommand SP_M_UpdateUser = new SqlCommand("SP_M_UpdateUser", sqlCon);
            SP_M_UpdateUser.CommandType = CommandType.StoredProcedure;

            SP_M_UpdateUser.Parameters.AddWithValue("@Id", employee.Id);
            SP_M_UpdateUser.Parameters.AddWithValue("@EmpName", employee.Name);
            SP_M_UpdateUser.Parameters.AddWithValue("@Gender", employee.Gender);
            SP_M_UpdateUser.Parameters.AddWithValue("@DOB", employee.DOB);
            SP_M_UpdateUser.Parameters.AddWithValue("@EmpAddress", employee.Address);
            SP_M_UpdateUser.Parameters.AddWithValue("@Tech", employee.TechId);
            SP_M_UpdateUser.Parameters.AddWithValue("@Branch", employee.CompanyBranchCode);
            SP_M_UpdateUser.Parameters.AddWithValue("@Marital", employee.MaritalStatus);
            SP_M_UpdateUser.Parameters.AddWithValue("@EmpDelete", false);
            SP_M_UpdateUser.Parameters.AddWithValue("@ActiveStatus", employee.ActiveStatus);

            SqlDataReader sqlData = SP_M_UpdateUser.ExecuteReader();
            while (sqlData.Read())
            {
                var strresult = sqlData["EmpName"].ToString();
            }

            sqlCon.Close();
            var strPassResult = "This " + employee.Name + " user Details are Updated Successfully";

            return Json(new
            {
                msg = strPassResult
            });
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Delete(Employee employee)
        {
            sqlCon.Open();

            SqlCommand SP_M_DeleteUser = new SqlCommand("SP_M_DeleteUser", sqlCon);
            SP_M_DeleteUser.CommandType = CommandType.StoredProcedure;
            SP_M_DeleteUser.Parameters.AddWithValue("@Id", employee.Id);

            SqlDataReader sqlData = SP_M_DeleteUser.ExecuteReader();

            sqlCon.Close();
            var strPassResult = "This " + employee.Name + " Account Deleted Successfully";

            return Json(new
            {
                msg = strPassResult
            });
        }
       
    }
}