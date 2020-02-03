using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using Employee_Info.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
        public void IntiateUserInPrajectMapping(string strEmail)
        {
            if (sqlCon.State == ConnectionState.Closed)
            {
                sqlCon.Open();
            }

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
                if (employee.Gender == "M")
                    employee.Gender = "Male";
                else if (employee.Gender == "F")
                    employee.Gender = "Female";
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
                employee.FilePath = dataReader["ImageFilePath"].ToString();
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

        [HttpGet]
        public ActionResult DownloadExcel()
        {
            ExcelPackage Ep = new ExcelPackage();
            ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
            Sheet.Cells["A1"].Value = "Name";
            Sheet.Cells["B1"].Value = "Gender";
            Sheet.Cells["C1"].Value = "Date of Birth";
            Sheet.Cells["D1"].Value = "Email";
            Sheet.Cells["E1"].Value = "Address";
            Sheet.Cells["F1"].Value = "Technology";
            Sheet.Cells["G1"].Value = "Branch";
            Sheet.Cells["H1"].Value = "Marital";
            Sheet.Cells["I1"].Value = "Active";
            Sheet.Cells["A1:I1"].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            Sheet.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(129, 212, 26));
            Sheet.Cells["A1:I1"].Style.Font.Color.SetColor(Color.Brown);
            Sheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            Sheet.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            Sheet.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            Sheet.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            int row = 2;

            JsonResult jsonResultOfAllEmployeeDetail = GetAllEmployee();
            string jsonSTRINGtOfAllEmployeeDetailWith_Key_msg = new JavaScriptSerializer().Serialize(jsonResultOfAllEmployeeDetail.Data);
            JObject obj = JObject.Parse(jsonSTRINGtOfAllEmployeeDetailWith_Key_msg);
            string jsonSTRINGtOfAllEmployeeDetail = obj["msg"].ToString();

            List<Employee> ListOfAllEmployeeDetail = JsonConvert.DeserializeObject<List<Employee>>(jsonSTRINGtOfAllEmployeeDetail);
            foreach (var employee in ListOfAllEmployeeDetail)
            {
                Sheet.Cells[string.Format("A{0}", row)].Value = employee.Name;
                Sheet.Cells[string.Format("B{0}", row)].Value = employee.Gender;
                Sheet.Cells[string.Format("C{0}", row)].Value = employee.StrDOB;
                Sheet.Cells[string.Format("D{0}", row)].Value = employee.Email;
                Sheet.Cells[string.Format("E{0}", row)].Value = employee.Address;
                Sheet.Cells[string.Format("F{0}", row)].Value = employee.TechId;
                Sheet.Cells[string.Format("G{0}", row)].Value = employee.CompanyBranchCode;
                Sheet.Cells[string.Format("H{0}", row)].Value = employee.MaritalStatus;
                Sheet.Cells[string.Format("I{0}", row)].Value = employee.ActiveStatus;
                row++;
            }
            row = row - 1;
            Color colFromHex = ColorTranslator.FromHtml("#B7DEE8");
            Sheet.Cells["A2:I" + row].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            Sheet.Cells["A2:I" + row].Style.Fill.BackgroundColor.SetColor(colFromHex);

            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader("content-disposition", "attachment: filename=" + "Report.xlsx");
            Response.BinaryWrite(Ep.GetAsByteArray());
            Response.End();

            return View();
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

        [HttpPost]
        [Route("Employee/UploadExcel")]
        public JsonResult UploadExcel()
        {
            string strresult = "", strPassResult = "";
            HttpPostedFileBase file = Request.Files["excelFileUpload"];

            if (file != null)   
            {
                string fileName = file.FileName;
                string fileContentType = file.ContentType;
                byte[] fileBytes = new byte[file.ContentLength];
                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));

                List<Employee> ReturnedByUploadedData = new List<Employee> { };

                using (var package = new ExcelPackage(file.InputStream))
                {
                    ExcelWorksheets currentSheet = package.Workbook.Worksheets;
                    ExcelWorksheet workSheet = currentSheet.First();
                    int noOfCol = workSheet.Dimension.End.Column;
                    int noOfRow = workSheet.Dimension.End.Row;
                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                    {
                        Employee employee = new Employee();

                        employee.Name = (workSheet.Cells[rowIterator, 1].Value ?? string.Empty).ToString();
                        employee.Gender = (workSheet.Cells[rowIterator, 2].Value ?? string.Empty).ToString();
                        if (string.Empty != (workSheet.Cells[rowIterator, 3].Value ?? string.Empty).ToString())
                        {
                            DateTime conv = DateTime.FromOADate(Convert.ToDouble(workSheet.Cells[rowIterator, 3].Value));
                            employee.DOB = Convert.ToDateTime(conv);
                            employee.StrDOB = conv.ToString("yyyy/MM/dd");
                        }
                        else
                        {
                            employee.DOB = new DateTime();
                        }
                        employee.Email = (workSheet.Cells[rowIterator, 4].Value ?? string.Empty).ToString();
                        employee.Address = (workSheet.Cells[rowIterator, 5].Value ?? string.Empty).ToString();
                        employee.TechId = (workSheet.Cells[rowIterator, 6].Value ?? string.Empty).ToString();
                        employee.CompanyBranchCode = (workSheet.Cells[rowIterator, 7].Value ?? string.Empty).ToString();
                        employee.MaritalStatus = (workSheet.Cells[rowIterator, 8].Value ?? string.Empty).ToString();
                        employee.FilePath = "";
                        if (employee.Name == string.Empty)
                        {
                            employee.ExcelUploadStatus = "Name field required";
                            ReturnedByUploadedData.Add(employee);
                        }
                        else if (employee.Gender == string.Empty)
                        {
                            employee.ExcelUploadStatus = "Gender field required";
                            ReturnedByUploadedData.Add(employee);
                        }
                        else if (string.Empty == (workSheet.Cells[rowIterator, 3].Value ?? string.Empty).ToString())
                        {
                            employee.ExcelUploadStatus = "Date of birth field required";
                            ReturnedByUploadedData.Add(employee);
                        }
                        else if (employee.Email == string.Empty)
                        {
                            employee.ExcelUploadStatus = "Email field required";
                            ReturnedByUploadedData.Add(employee);
                        }
                        else if (employee.Gender == string.Empty)
                        {
                            employee.ExcelUploadStatus = "Gender field required";
                            ReturnedByUploadedData.Add(employee);
                        }
                        else if (employee.Address == string.Empty)
                        {
                            employee.ExcelUploadStatus = "Address field required";
                            ReturnedByUploadedData.Add(employee);
                        }
                        else if (employee.TechId == string.Empty)
                        {
                            employee.ExcelUploadStatus = "Technology field required";
                            ReturnedByUploadedData.Add(employee);
                        }
                        else if (employee.CompanyBranchCode == string.Empty)
                        {
                            employee.ExcelUploadStatus = "Branch field required";
                            ReturnedByUploadedData.Add(employee);
                        }
                        else if (employee.MaritalStatus == string.Empty)
                        {
                            employee.ExcelUploadStatus = "Marital Status field required";
                            ReturnedByUploadedData.Add(employee);
                        }
                        else
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
                            SP_M_InsertUser.Parameters.AddWithValue("@ImageFilePath", employee.FilePath);
                            SP_M_InsertUser.Parameters.AddWithValue("@EmpDelete", false);
                            SP_M_InsertUser.Parameters.AddWithValue("@ActiveStatus", "Active");

                            SqlDataReader sqlData = SP_M_InsertUser.ExecuteReader();
                            while (sqlData.Read())
                            {
                                strresult = sqlData["result"].ToString();
                            }
                            sqlCon.Close();

                            if (strresult == "AlreadyExist")
                            {
                                strPassResult = "This " + employee.Email + " user already Exist";
                                employee.ExcelUploadStatus = "User Already Exist";
                                ReturnedByUploadedData.Add(employee);

                            }
                            else
                            {
                                IntiateUserInPrajectMapping(employee.Email);
                                strPassResult = "This " + employee.Email + " user added succefully";
                                employee.ExcelUploadStatus = "User Added Success";
                                ReturnedByUploadedData.Add(employee);
                            }
                        }
                    }
                }

                //Generate excel for uploaded data

                ExcelPackage Ep = new ExcelPackage();
                ExcelWorksheet Sheet = Ep.Workbook.Worksheets.Add("Report");
                Sheet.Cells["A1"].Value = "Name";
                Sheet.Cells["B1"].Value = "Gender";
                Sheet.Cells["C1"].Value = "Date of Birth";
                Sheet.Cells["D1"].Value = "Email";
                Sheet.Cells["E1"].Value = "Address";
                Sheet.Cells["F1"].Value = "Technology";
                Sheet.Cells["G1"].Value = "Branch";
                Sheet.Cells["H1"].Value = "Marital";
                Sheet.Cells["I1"].Value = "Active";
                Sheet.Cells["J1"].Value = "Status of Process";
                Sheet.Cells["A1:j1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells["A1:j1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(129, 212, 26));
                Sheet.Cells["A1:j1"].Style.Font.Color.SetColor(Color.Brown);
                Sheet.Cells.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                Sheet.Cells.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                Sheet.Cells.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                Sheet.Cells.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                int row = 2;

                foreach (var employee in ReturnedByUploadedData)
                {
                    Sheet.Cells[string.Format("A{0}", row)].Value = employee.Name;
                    Sheet.Cells[string.Format("B{0}", row)].Value = employee.Gender;
                    Sheet.Cells[string.Format("C{0}", row)].Value = employee.StrDOB;
                    Sheet.Cells[string.Format("D{0}", row)].Value = employee.Email;
                    Sheet.Cells[string.Format("E{0}", row)].Value = employee.Address;
                    Sheet.Cells[string.Format("F{0}", row)].Value = employee.TechId;
                    Sheet.Cells[string.Format("G{0}", row)].Value = employee.CompanyBranchCode;
                    Sheet.Cells[string.Format("H{0}", row)].Value = employee.MaritalStatus;
                    Sheet.Cells[string.Format("I{0}", row)].Value = employee.ActiveStatus;
                    Sheet.Cells[string.Format("J{0}", row)].Value = employee.ExcelUploadStatus;
                    row++;
                }
                Sheet.Column(1).AutoFit();
                Sheet.Column(2).AutoFit();
                Sheet.Column(3).AutoFit();
                Sheet.Column(5).AutoFit();
                Sheet.Column(6).AutoFit();
                Sheet.Column(7).AutoFit();
                Sheet.Column(8).AutoFit();
                Sheet.Column(9).AutoFit();
                Sheet.Column(10).AutoFit();

                row = row;
                Sheet.Cells["J2:J" + row].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells["J2:J" + row].Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                Color colFromHex = ColorTranslator.FromHtml("#B7DEE8");
                Sheet.Cells["A2:I" + row].Style.Fill.PatternType = ExcelFillStyle.Solid;
                Sheet.Cells["A2:I" + row].Style.Fill.BackgroundColor.SetColor(colFromHex);

                var filePath = System.Web.Hosting.HostingEnvironment.MapPath("~/SourceExcel/" + "StatusOfUploadData.xlsx");
                FileInfo excelFile = new FileInfo(@filePath);
                Ep.SaveAs(excelFile);
                Response.End();
            }
            return Json(null);
        }

        public FileResult DownloadexcelAutomatic()
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(@"E:\Sivaramkumar\Git\Employee_Info\Employee_Info\SourceExcel\StatusOfUploadData.xlsx");
            string fileName = "StatusOfUploadData.xlsx";

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection formCollection)
        {
            HttpPostedFileBase ImageFile = Request.Files["FileUpload"];
            string StrEmployeeJson = formCollection["Employee"];

            Employee employee = JsonConvert.DeserializeObject<Employee>(StrEmployeeJson);
            string path = "";
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];
                var fileName = Path.GetFileName(file.FileName);
                path = Path.Combine(Server.MapPath("~/Content/images/"), fileName);
                file.SaveAs(path);
            }


            string firstStrSlash = path.Substring(0, path.LastIndexOf('\\'));
            string SecondStrSlash = path.Substring(0, firstStrSlash.LastIndexOf('\\'));
            path = path.Substring(SecondStrSlash.LastIndexOf('\\'), path.Length - SecondStrSlash.LastIndexOf('\\'));

            path = path.Replace('\\', '/');
            employee.FilePath = "./.." + path;

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
            SP_M_InsertUser.Parameters.AddWithValue("@ImageFilePath", employee.FilePath);
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