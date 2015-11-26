using MVC5_full_version.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC5_full_version;

namespace MVC5_full_version.Controllers
{
    public class TablesController : Controller
    {
        // GET: Tables
        public ActionResult BasicTables()
        {
            return View();
        }
        public ActionResult DataTables()
        {
            return View();
        }
        public JsonResult Autocomplete(string term)
        {
            DbConnect con = new DbConnect();
            List<Address> empResult = new List<Address>();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            // cmd.CommandText = "select Top 20  user_area +', '+[user_dist] as user_dist from [dbo].[user_address_detail] where [user_dist] LIKE ''+@SearchEmpName+'%'  or user_area like ''+@area+'%' ";
            cmd.CommandText = "select * from [dbo].[user_address_detail] where [user_dist] LIKE ''+@SearchEmpName+'%'";

            cmd.Parameters.AddWithValue("@SearchEmpName", term);
            // cmd.Parameters.AddWithValue("@area", empAddress);
            dt = con.GetDataTable(cmd);
            List<Address> adds = DataTableToObject(dt).GroupBy(p => p.user_dist).Select(g => g.First()).ToList();

            return Json(adds, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDesignation(string term)
        {
            DbConnect con = new DbConnect();
            List<Designation> empResult = new List<Designation>();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            // cmd.CommandText = "select Top 20  user_area +', '+[user_dist] as user_dist from [dbo].[user_address_detail] where [user_dist] LIKE ''+@SearchEmpName+'%'  or user_area like ''+@area+'%' ";
            cmd.CommandText = "select  user_exp_id, [user_job_title] from [dbo].[user_exp] where [user_job_title] LIKE '%'+@SearchEmpName+'%'";
            cmd.Parameters.AddWithValue("@SearchEmpName", term);
            // cmd.Parameters.AddWithValue("@area", empAddress);
            dt = con.GetDataTable(cmd);
            List<Designation> adds = DataTableToDesignation(dt).GroupBy(p =>p.user_job_title).Select(g => g.First()).ToList();
            return Json(adds, JsonRequestBehavior.AllowGet);
        }

        private List<Designation> DataTableToDesignation(DataTable dt)
        {
            List<Designation> Detail = new List<Designation>();
            foreach (DataRow item in dt.Rows)
            {
                Designation empResult = new Designation();
                empResult.user_exp_id = Convert.ToInt32(item["user_exp_id"].ToString());
                empResult.user_job_title = item["user_job_title"].ToString();
                Detail.Add(empResult);
            }
            return Detail;
        }

        public JsonResult  GetEmployeeSkills(string empSkills)
        {
            List<Skilled> empResult = new List<Skilled>();
            DbConnect con = new DbConnect();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "SELECT user_skill_id, [user_skill] FROM [dbo].[user_skills] where [user_skill] LIKE ''+@SearchempSkills+'%'";
            cmd.Parameters.AddWithValue("@SearchempSkills", empSkills);
            dt = con.GetDataTable(cmd);

            empResult = DataTableToSkilled(dt).GroupBy(p=>p.user_skill.Split(',')).Select(g=>g.First()).ToList();
            return Json(empResult, JsonRequestBehavior.AllowGet);
        }
        private static List<Address> DataTableToObject(DataTable dt)
        {
            List<Address> Detail = new List<Address>();
            foreach (DataRow item in dt.Rows)
            {
                Address empResult = new Address();
                empResult.user_address_id =Convert.ToInt32(item["user_address_id"].ToString());
                empResult.user_address1 = item["user_address1"].ToString();
                empResult.user_zipcode = item["user_zipcode"].ToString();
                empResult.user_area = item["user_area"].ToString();
                empResult.user_dist = item["user_dist"].ToString();
                empResult.user_country = item["user_country"].ToString();              
                Detail.Add(empResult);
            }
            return Detail;
        }
        private static List<Skilled> DataTableToSkilled(DataTable dt)
        {
            List<Skilled> Detail = new List<Skilled>();
            foreach (DataRow item in dt.Rows)
            {
                Skilled empResult = new Skilled();
                empResult.user_skill_id = Convert.ToInt32(item["user_skill_id"].ToString());
                empResult.user_skill = item["user_skill"].ToString();              
                Detail.Add(empResult);
            }
            return Detail;
        }

        public JsonResult GetFindResult(string what, string where)
        {
            List<string> empResult = new List<string>();
            DbConnect con = new DbConnect();
            DataTable dt = new DataTable();
            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = "[dbo].[FindData]";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@user_job_title", what);
            cmd.Parameters.AddWithValue("@user_area", where);
            cmd.Parameters.AddWithValue("@user_dist", where);
            cmd.Parameters.AddWithValue("@user_skill", what);
            dt = con.GetDataTable(cmd);
            try
            {
                var result = from tab in dt.AsEnumerable()
                             group tab by tab["user_id"]
                                 into resultDT
                                 select new
                                 {
                                     user_id = resultDT.Key,
                                     user_full_name = resultDT.First()["user_full_name"].ToString(),
                                     sex = resultDT.First()["sex"].ToString(),
                                     user_dist = resultDT.First()["user_dist"].ToString(),
                                     user_area = resultDT.First()["user_area"].ToString(),
                                     user_skill = resultDT.First()["user_skill"].ToString(),
                                     user_country = resultDT.First()["user_country"].ToString(),
                                     user_job_title = resultDT.First()["user_job_title"].ToString(),
                                     user_exp_duration = resultDT.First()["user_exp_duration"].ToString(),
                                     user_company_name = resultDT.First()["user_company_name"].ToString(),
                                     user_address1 = resultDT.First()["user_address1"].ToString(),
                                     user_exp_from = resultDT.First()["user_exp_from"].ToString(),
                                     Experienced = Convert.ToInt32(resultDT.First()["Experienced"].ToString()),

                                 };
                List<GetResult> results= result.ToDataTable().ToList<GetResult>();
                return Json(results, JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}