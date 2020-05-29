using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using cw4.DAL;
using cw4.DTOs.Requests;
using cw4.DTOs.Responses;
using cw4.Models;
using cw4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace cw4.Controllers
{
    [Route("api/enrollments")]
    [ApiController] //-> implicit model validation
    public class EnrollmentsController : ControllerBase
    {
        private IStudentDBService _service;


        public EnrollmentsController(IStudentDBService service)
        {
            _service = service;
        }

        
        [HttpPost]
        [Authorize(Roles = "employee")]
        public IActionResult EnrollStudent(EnrollStudentRequest request){
 
            if (!ModelState.IsValid) 
            {
                 var d = ModelState;
                 return BadRequest("!!!");
             }
            
       
        var response = _service.EnrollStudent(request);

        if (response.Status == 201)
        {
            return CreatedAtAction(response.Message, response.enrollment);
        }

        
            return BadRequest(response.Message);
        
          

    
        }


        [HttpPost("/api/enrollments/promotions")]
        [Authorize(Roles = "employee")]
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            
            if (!ModelState.IsValid)
            {
                var d = ModelState;
                return BadRequest("!!!");
            }

            // using (var con = new SqlConnection(ConString))
            // {
            //     using (var com = new SqlCommand())
            //     {
            //         com.Connection = con;
            //         con.Open();
            //
            //         var transaction = con.BeginTransaction();
            //         com.Transaction = transaction;
            //         
            //         //1. Sprawdzam czy w tabeli enrollment istnieje wpis o podanej wartości Studies i Semester, W przeciwnym razie zwracam kod 404 Not Found
            //         
            //         com.CommandText = "SELECT * FROM Enrollment" +
            //                           " INNER JOIN Studies" +
            //                           " ON Studies.IdStudy = Enrollment.IdStudy" +
            //                           " WHERE Enrollment.Semester = @semester" +
            //                           " AND Studies.Name = @studies";
            //         com.Parameters.AddWithValue("semester", request.Semester);
            //         com.Parameters.AddWithValue("studies", request.Studies);
            //
            //         var dr = com.ExecuteReader();
            //
            //         if (!dr.Read())
            //         {
            //             dr.Close();
            //
            //             return new NotFoundResult();
            //         }
            //         dr.Close();
            //         
            //         // Jeżeli wszystko poszło dobrze uruchamiam procedurę składową
            //
            //         com.CommandText = "promoteStudents";
            //         com.CommandType = CommandType.StoredProcedure;
            //         dr = com.ExecuteReader();
            //         if (dr.Read())
            //         {
            //             enrollment.IdEnrollment = (int) dr["IdEnrollment"];
            //             enrollment.Semester = (int) dr["Semester"];
            //             enrollment.IdStudy = (int) dr["IdStudy"];
            //             enrollment.StartDate = (DateTime) dr["StartDate"];
            //             dr.Close();
            //         }
            //
            //
            //     }
            // }

            var response = _service.PromoteStudent(request);
            if (response.Status == 404)
            {
                return NotFound();
            }
            
            

            return CreatedAtAction(response.Message, response.enrollment);
        }
        
    }
}