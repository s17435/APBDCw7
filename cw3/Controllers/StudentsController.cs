using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using cw4.DAL;
using cw4.DTOs.Requests;
using cw4.Models;
using cw4.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cw4.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {
        // Do połączenia się z bazą danych wykorzystałem Dockera
        // Połączenie do MS SQL przez VPN na MACu niestety mnie przerosło
        private const string ConString = "Server=localhost,1433; Database=Master; User Id=SA; Password= Pa55w0rd";
        //private const string ConString = "Server=db-mssql.pjwstk.edu.pl; Initial Catalog=s17435; User ID=PJWSTK/s17435; Password= ";
        //private const string ConString = "Server=jdbc:jtds:sqlserver://db-mssql.pjwstk.edu.pl/s17435; Initial Catalog=s17435; User ID=s17435; Password= ";




        public IConfiguration Configuration { get; set; }

        private IStudentDBService _dbService;


        public StudentsController(IConfiguration configuration, IStudentDBService dbService)
        {
            Configuration = configuration;
            _dbService = dbService;
      


        }






        [HttpGet]
        public IActionResult GetStudent(string orderBy = "Nazwisko")


        {
            List<Student> list = new List<Student>();
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "SELECT IndexNumber, FirstName, LastName, BirthDate, Name, Semester FROM Student INNER JOIN Enrollment E on Student.IdEnrollment = E.IdEnrollment INNER JOIN Studies S on E.IdStudy = S.IdStudy";

                con.Open();
                SqlDataReader sqlDataReader = com.ExecuteReader();

                while (sqlDataReader.Read())
                {
                    Student student = new Student();
                    student.IndexNumber = sqlDataReader["IndexNumber"].ToString();

                    student.FirstName = sqlDataReader["FirstName"].ToString();
                    student.LastName = sqlDataReader["LastName"].ToString();
                    student.BirthDate = (DateTime) sqlDataReader["BirthDate"];

                   
                    list.Add(student);
                }

                con.Close();
            }

            
            return Ok(list);
        }




        // Używając polecenia np: https://localhost:5001/api/students/2; DROP TABLE STUDENTS
        // Bardzo  łatwo usunąć tabelę

        [HttpGet("{id}")]
        public IActionResult GetStudentById(string id)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
         
                com.CommandText = "SELECT IndexNumber, FirstName, LastName, BirthDate, Name, Semester FROM Student INNER JOIN Enrollment E on Student.IdEnrollment = E.IdEnrollment INNER JOIN Studies S on E.IdStudy = S.IdStudy WHERE IndexNumber = " + @id;
                com.Parameters.AddWithValue("id", id);
                con.Open();
                SqlDataReader sqlDataReader = com.ExecuteReader();
               

                if(sqlDataReader.Read())
                {
                    Student student = new Student();
                    student.IndexNumber = sqlDataReader["IndexNumber"].ToString();

                    student.FirstName = sqlDataReader["FirstName"].ToString();
                    student.LastName = sqlDataReader["LastName"].ToString();
                    student.BirthDate = (DateTime)sqlDataReader["BirthDate"];





                    return Ok(student);
                } else
                { return NotFound("Nie ma takiego ucznia"); }

            }
        }



        [HttpGet("en/{id}")]
        public IActionResult GetEnrollmentByStudentId(string id)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = "SELECT Name, Semester FROM Student INNER JOIN Enrollment E on Student.IdEnrollment = E.IdEnrollment INNER JOIN Studies S on E.IdStudy = S.IdStudy WHERE IndexNumber = " + @id;
                com.Parameters.AddWithValue("id", id);
                con.Open();
                SqlDataReader sqlDataReader = com.ExecuteReader();

                if (sqlDataReader.Read())
                {
         
                    String response = "Semestr: "+ sqlDataReader["Semester"].ToString() + " Studia: " + sqlDataReader["Name"].ToString();
         




                    return Ok(response);
                }
                else
                { return NotFound("Nie ma takiego ucznia"); }

            }
        }



        [HttpPost]
        public IActionResult Login(LoginRequest request)
        {

            string result = _dbService.Login(request);
            if (result.Contains("Blad:"))
                return Unauthorized("require login and password");



            var claims = new[] {

                new Claim(ClaimTypes.NameIdentifier,"1"),
                new Claim(ClaimTypes.Name, "jan123"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student")
            };

            
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["!!81Secret27bwsdfgh8"]));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var token = new JwtSecurityToken
            (
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
            );

            var jwt = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            };

            string s = _dbService.UpdateToken(jwt.refreshToken,request.Login);


            return Ok(jwt);

        }


       
        [HttpPost("refreshjwt/{token}")]
        public IActionResult Refresh(string token)
        {
            var result = _dbService.RefreshToken(token);
            if (result.Contains("Blad:"))
            {
                return Unauthorized("Zły JWT");
            }
            
            
            
            var claims = new[] {

                new Claim(ClaimTypes.NameIdentifier,"1"),
                new Claim(ClaimTypes.Name, "jan123"),
                new Claim(ClaimTypes.Role, "admin"),
                new Claim(ClaimTypes.Role, "student"),
                new Claim(ClaimTypes.Role, "employee")
            };
            
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["!!81Secret27bwsdfgh8"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            var tmptoken = new JwtSecurityToken(
                issuer: "Gakko",
                audience: "Students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: credentials
                
                );

            var jwt = new
            {
                token = new JwtSecurityTokenHandler().WriteToken(tmptoken),
                refreshToken = Guid.NewGuid()
            };

            return Ok(jwt);
        }



    }
}
