using cw4.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cw4.DTOs.Responses;
using cw4.Models;

namespace cw4.Services
{
    public interface IStudentDBService
    {
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
        PromoteStudentsResponse PromoteStudent(PromoteStudentsRequest request);

        Student GetStudent(string index);

        string Login(LoginRequest request);

        string UpdateToken(Guid refreshToken, string id);

        string RefreshToken(string refresh);

    }
}