using Employee_Info.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Employee_Info.Controllers
{
    public class ProjectController : Controller
    {
        SqlConnection sqlCon = new SqlConnection("Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=EmpDB;Integrated Security=True");

        public JsonResult GetEmpNames()
        {
            sqlCon.Open();


            SqlCommand SP_M_SelectAllUser = new SqlCommand("SP_M_SelectAllUser", sqlCon);
            SP_M_SelectAllUser.CommandType = CommandType.StoredProcedure;
            SqlDataReader dataReader = SP_M_SelectAllUser.ExecuteReader();

            List<Employee> employees = new List<Employee>();

            while (dataReader.Read())
            {
                Employee employee = new Employee();

                employee.Id = Convert.ToInt32(dataReader["Id"]);
                employee.Name = dataReader["EmpName"].ToString();
                employees.Add(employee);
            }

            sqlCon.Close();

            return Json(employees, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Manager")]
        public ActionResult CreateProject()
        {
            return View();
        }

        [Authorize(Roles = "Manager")]
        public ActionResult IntiateProject(int id)
        {
            sqlCon.Open();
            int ProjectId = 0;
            SqlCommand SP_M_ProjectMapping_ExistOrNot = new SqlCommand("SP_M_ProjectMapping_ExistOrNot", sqlCon);
            SP_M_ProjectMapping_ExistOrNot.CommandType = CommandType.StoredProcedure;

            SP_M_ProjectMapping_ExistOrNot.Parameters.AddWithValue("@Id", id);
            SqlDataReader sqlData = SP_M_ProjectMapping_ExistOrNot.ExecuteReader();
            string strresult = "";
            while (sqlData.Read())
            {
                strresult = sqlData["result"].ToString();
            }
            sqlData.Close();
            if (strresult == "New")
            {

                ProjectId = GenarateProjectId();

                string strKey = ProjectId.ToString() + ",";
                SqlCommand SP_M_ProjectMapping_update = new SqlCommand("insert into Tb_M_ProjectMapping values(" + id + ", '" + strKey + "')", sqlCon);
                SP_M_ProjectMapping_update.ExecuteNonQuery();
            }
            else
            {
                ProjectId = GenarateProjectId();

                SqlCommand select_Project_Keys = new SqlCommand("select * from Tb_M_ProjectMapping where Id = '" + id + "'", sqlCon);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(select_Project_Keys);
                DataSet Ds = new DataSet();
                sqlDataAdapter.Fill(Ds);

                int empid = Convert.ToInt32(Ds.Tables[0].Rows[0]["Id"]);
                string strGrpOfProjectId = Ds.Tables[0].Rows[0]["ProjectIds"].ToString();

                strGrpOfProjectId = strGrpOfProjectId + ProjectId;

                SqlCommand SP_M_ProjectMapping_update = new SqlCommand("SP_M_ProjectMapping_update", sqlCon);
                SP_M_ProjectMapping_update.CommandType = CommandType.StoredProcedure;
                SP_M_ProjectMapping_update.Parameters.AddWithValue("@Id", id);
                SP_M_ProjectMapping_update.Parameters.AddWithValue("@ProjectIds", strGrpOfProjectId + ",");
                SP_M_ProjectMapping_update.ExecuteNonQuery();
            }


            SqlCommand SP_M_Project = new SqlCommand("SP_M_Project", sqlCon);
            SP_M_Project.CommandType = CommandType.StoredProcedure;

            SP_M_Project.Parameters.AddWithValue("@Id", ProjectId);
            SP_M_Project.Parameters.AddWithValue("@TaskName", "");
            SP_M_Project.Parameters.AddWithValue("@BeginDate", "");
            SP_M_Project.Parameters.AddWithValue("@ProPriority", "");
            SP_M_Project.Parameters.AddWithValue("@ProStatus", "");
            SP_M_Project.Parameters.AddWithValue("@Resolution", "");
            SP_M_Project.Parameters.AddWithValue("@ProDescription", "");
            SP_M_Project.Parameters.AddWithValue("@IsDeleted", 0);
            SP_M_Project.ExecuteNonQuery();

            sqlCon.Close();

            return Json(new
            {
                url = Url.Action("", "Project/CreateProject"),
                ProId = ProjectId.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        private int GenarateProjectId()
        {
            int ProjectId = 0;
            SqlCommand select_All_Project_Keys = new SqlCommand("select * from Tb_M_Project", sqlCon);
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(select_All_Project_Keys);
            DataTable Tb_M_Project = new DataTable();
            sqlDataAdapter.Fill(Tb_M_Project);

            if (Tb_M_Project.Rows.Count >= 1)
            {
                int tempHighestValue = 0;
                for (int row = 0; row < Tb_M_Project.Rows.Count; row++)
                {
                    string strProjectId = Tb_M_Project.Rows[row][0].ToString();
                    if(tempHighestValue < Convert.ToInt32(strProjectId))
                    {
                        tempHighestValue = Convert.ToInt32(strProjectId);
                    }
                }

                ProjectId = Convert.ToInt32(tempHighestValue) + 1;
            }
            else
            {
                ProjectId = 1000;
            }

            return ProjectId;
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        public ActionResult CreateProject(ProjectCreation projectCreation)
        {
            sqlCon.Open();

            SqlCommand SP_M_Project_update = new SqlCommand("SP_M_Project_update", sqlCon);
            SP_M_Project_update.CommandType = CommandType.StoredProcedure;

            SP_M_Project_update.Parameters.AddWithValue("@Id", projectCreation.ProId);
            SP_M_Project_update.Parameters.AddWithValue("@TaskName", projectCreation.TaskName);
            SP_M_Project_update.Parameters.AddWithValue("@BeginDate", projectCreation.BeginDate);
            SP_M_Project_update.Parameters.AddWithValue("@ProPriority", projectCreation.Priority);
            SP_M_Project_update.Parameters.AddWithValue("@ProStatus", projectCreation.Status);
            SP_M_Project_update.Parameters.AddWithValue("@Resolution", projectCreation.Resolution);
            SP_M_Project_update.Parameters.AddWithValue("@ProDescription", projectCreation.DescCmt);
            SP_M_Project_update.Parameters.AddWithValue("@IsDeleted", 0);

            int result = SP_M_Project_update.ExecuteNonQuery();
            string strPassResult = "", strurl = "";
            if (result >= 1)
            {
                strPassResult = "Project Created Successfully";
                strurl = Url.Action("", "Project/CreateProject");
            }
            else
            {
                strPassResult = "Project not Create Successfully";
            }

            sqlCon.Close();

            return Json(new
            {
                msg = strPassResult ,
                url = strurl,
                proId = projectCreation.EmpId
            });
        }

        //[Authorize(Roles = "Manager,Employee")]
        [HttpGet]
        public JsonResult getProjectDetails(int empId)
        {
            sqlCon.Open();

            SqlCommand SP_M_getProjectid_ProjectMapping = new SqlCommand("SP_M_getProjectid_ProjectMapping", sqlCon);
            SP_M_getProjectid_ProjectMapping.CommandType = CommandType.StoredProcedure;

            SP_M_getProjectid_ProjectMapping.Parameters.AddWithValue("@Id", empId);
            SqlDataReader dataReader = SP_M_getProjectid_ProjectMapping.ExecuteReader();

            string strGroupOFProjectId = "";
            while (dataReader.Read())
            {
                strGroupOFProjectId = dataReader["ProjectIds"].ToString();
            }
            dataReader.Close();
            sqlCon.Close();

            string[] GroupOFProjectIds = strGroupOFProjectId.Split(',');

            List<ProjectCreation> LSProjects = new List<ProjectCreation> { };

            for(int i = 0; i <= GroupOFProjectIds.Length - 2; i++)
            {
                SqlCommand SP_M_getProjectDetails = new SqlCommand("SP_M_getProjectDetails", sqlCon);
                SP_M_getProjectDetails.CommandType = CommandType.StoredProcedure;
                SP_M_getProjectDetails.Parameters.AddWithValue("@Id", GroupOFProjectIds[i]);
                sqlCon.Open();

                SqlDataReader dataReader_ProjectDetails = SP_M_getProjectDetails.ExecuteReader();
                while (dataReader_ProjectDetails.Read())
                {
                    ProjectCreation project = new ProjectCreation();

                    project.ProId = Convert.ToInt32(dataReader_ProjectDetails["Id"].ToString());
                    project.TaskName = dataReader_ProjectDetails["TaskName"].ToString();
                    var strBegindate = Convert.ToDateTime(dataReader_ProjectDetails["BeginDate"].ToString());
                    string[] strMonth = new string[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                    project.strBeginDate = strBegindate.Day + " " + strMonth[strBegindate.Month - 1] + " " + strBegindate.Year;
                    project.Priority = dataReader_ProjectDetails["ProPriority"].ToString();
                    project.Status = dataReader_ProjectDetails["ProStatus"].ToString();
                    project.Resolution = dataReader_ProjectDetails["Resolution"].ToString();
                    project.DescCmt = dataReader_ProjectDetails["ProDescription"].ToString();
                    LSProjects.Add(project);
                }
                dataReader_ProjectDetails.Close();
                sqlCon.Close();
            }

            return Json(new { msg = LSProjects }, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        public JsonResult Delete(ProjectCreation projectCreation)
        {
            string strPassResult = "", strurl = "";
            try
            {
                sqlCon.Open();

                SqlCommand select_Project_Keys = new SqlCommand("select * from Tb_M_ProjectMapping where Id = '" + projectCreation.EmpId + "'", sqlCon);

                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(select_Project_Keys);
                DataSet Ds = new DataSet();
                sqlDataAdapter.Fill(Ds);

                int empid = Convert.ToInt32(Ds.Tables[0].Rows[0]["Id"]);
                string strGrpOfProjectId = Ds.Tables[0].Rows[0]["ProjectIds"].ToString();

                string[] GrpOfProjectIds = strGrpOfProjectId.Split(',');
                if (GrpOfProjectIds.Contains(projectCreation.ProId.ToString()))
                {
                    List<string> tempGrpOfProjectId = GrpOfProjectIds.ToList();
                    int tempindex = tempGrpOfProjectId.FindIndex(pro => pro == projectCreation.ProId.ToString());
                    tempGrpOfProjectId.RemoveAt(tempindex);
                    strGrpOfProjectId = string.Join(",", tempGrpOfProjectId);
                }

                SqlCommand SP_M_Project_delete = new SqlCommand("SP_M_Project_delete", sqlCon);
                SP_M_Project_delete.CommandType = CommandType.StoredProcedure;
                SP_M_Project_delete.Parameters.AddWithValue("@Id", projectCreation.ProId);
                SP_M_Project_delete.Parameters.AddWithValue("@IsDeleted", 1);

                int resultProject = SP_M_Project_delete.ExecuteNonQuery();

                SqlCommand SP_M_ProjectMapping_update = new SqlCommand("SP_M_ProjectMapping_update", sqlCon);
                SP_M_ProjectMapping_update.CommandType = CommandType.StoredProcedure;
                SP_M_ProjectMapping_update.Parameters.AddWithValue("@Id", projectCreation.EmpId);
                SP_M_ProjectMapping_update.Parameters.AddWithValue("@ProjectIds", strGrpOfProjectId);
                int resultProjectMapping = SP_M_ProjectMapping_update.ExecuteNonQuery();

                sqlCon.Close();

                if (resultProject > 0 && resultProjectMapping > 0)
                {
                    strPassResult = "Project deleted Successfully";
                    strurl = Url.Action("", "Project/CreateProject");
                }
                else
                {
                    strPassResult = "Project not deleted Successfully";
                }
            }
            catch (Exception e)
            {
                strPassResult = e.Message;
            }
            return Json(new { 
                msg = strPassResult,
                url = strurl
                });
        }

        [Authorize(Roles = "Employee")]
        [HttpGet]
        public ActionResult EmployeeAllDetail(int nEmpId)
        {
            return View();
        }
    }
}