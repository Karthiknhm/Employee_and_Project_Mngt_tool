using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Employee_Info.Models
{
	public class Employee
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Gender { get; set; }

        public string Email { get; set; }

		public DateTime DOB { get; set; }

		public string StrDOB { get; set; }

		public string Address { get; set; }

		public string TechId { get; set; }

		public string CompanyBranchCode { get; set; }

		public string MaritalStatus { get; set; }

		public string ActiveStatus { get; set; }

		public bool isDeleted { get; set; }

	}

	public class Technology
	{
		public int TechId { get; set; }

		public string TechName { get; set; }
	}

	public class Branch
	{
		public int Id { get; set; }

		public string Name { get; set; }
	}

    public class Register
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public int RoleId { get; set; }

    }

    public class Role 
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; }
    }

    public class QueryForm
    {
        public string Email { get; set; }

        public string QueryComment { get; set; }
    }

    public class ProjectCreation
    {
        public int EmpId { get; set; }
        public int ProId { get; set; }
        public string TaskName { get; set; }
        public DateTime BeginDate { get; set; }
        public string strBeginDate { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string Resolution { get; set; }
        public string DescCmt { get; set; }

    }
}