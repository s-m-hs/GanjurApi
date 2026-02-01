using System.Security.Claims;
using AutoMapper;
using CY_BM;
using CY_DM;
using CY_WebApi.Models;
using CY_WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly JwtService _jwtService;
        private readonly CyContext _db;
        private readonly IMapper _mapper;

        public TaskController(JwtService jwt, CyContext db, IMapper mapper)
        {
            _jwtService = jwt;
            _db = db;
            _mapper = mapper;

        }

        [Authorize]
        [HttpPost("addTask")]
        async public Task<ActionResult> addTask([FromBody] TaskDTO dto)
        {
            var userClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaim, out int userId)) return Unauthorized(new { msg = "unAutorized" });
            //if (!int.TryParse(userClaim, out int userId))
            //{
            //    userId = 4;
            //}

            if (dto == null) return BadRequest(new { msg = "no Input" });
            if (dto.UserId == userId) return BadRequest(new { msg = "مسؤل انجام تسک با ایجاد کننده تسک یکسان است ..." });

            CyTask newTask = new CyTask()
            {
                IsVisible = true,
                CreateDate = DateTime.Now,
                Title = dto.Title,
                CompletionDate = dto.CompletionDate,
                Description = dto.Description,
                Hidden = false,
                TaskKind = dto.TaskKind,
                Score = Score.VeryBad,
                TaskState = TaskState.Wating,
                AdminId = userId,///ایجادکننده تسک
                UserId = dto.UserId,///مسول تسک
                Color = dto.Color,
                Important = dto.Important,
            };

            await _db.CyTask.AddAsync(newTask);
            await _db.SaveChangesAsync();


            return Ok(dto);

        }

        [TypeAuthorize([UserType.SysAdmin])]
        [HttpGet("getAllTaskS")]
        async public Task<ActionResult> getAllTaskS(int? taskId)
        {
            var query = _db.CyTask.Where(x => x.IsVisible);
            List<CyTask> Task = new();

            if (taskId == null)
            {
                Task = query.ToList();

            }
            else
            {
                Task = query.Where(x => x.ID == taskId).ToList();
            }

            return Ok(Task);
        }

        [Authorize]
        [HttpGet("admin&UserTasks")]

        async public Task<ActionResult> adminUserTasks(bool show = true)
        {
            var userClaims = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaims, out int adminId)) return Unauthorized();
            //if (!int.TryParse(userClaims, out int adminId))
            //{
            //    adminId = 4;
            //}
            var query = _db.CyTask.Where(x => x.IsVisible && (x.AdminId == adminId || x.UserId == adminId));

            ///// عدم نمایش همه تسگها
            if (!show)
            {
                query = query.Where(x => x.Hidden == false);
            }

            var tasks = query.
                OrderByDescending(u => u.AdminId).OrderByDescending(u => u.CompletionDate).ToList();

            if (!tasks.Any()) return Ok(tasks);

            List<TaskDTO> dtoList = new();
            foreach (var item in tasks)
            {
                var dto = _mapper.Map<TaskDTO>(item);
                dtoList.Add(dto);

            }

            return Ok(dtoList);

        }

        [Authorize]
        [HttpGet("todayTasks")]
        async public Task<ActionResult> todayTasks()
        {
            var userClaims = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaims, out int userId)) return Unauthorized();
            //if (!int.TryParse(userClaims, out int userId))
            //{
            //    userId = 4;
            //}


            var today = DateTime.Today;
            var tasks = await _db.CyTask.
                Where(x => x.IsVisible && x.Hidden==false &&  x.CompletionDate.HasValue && x.CompletionDate.Value.Date == today && (x.AdminId == userId || x.UserId == userId)).ToArrayAsync();

            if (!tasks.Any()) return Ok(tasks);

            List<TaskDTO> dtoList = new();
            foreach (var item in tasks)
            {
                var dto = _mapper.Map<TaskDTO>(item);
                dtoList.Add(dto);

            }
            return Ok(dtoList);



        }

        [Authorize]
        [HttpPut("editeTask")]

        async public Task<ActionResult> editeTask([FromBody] TaskDTO dto)
        {
   
      
            var userClaims = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaims, out int adminId)) return Unauthorized();
            //if (!int.TryParse(userClaims, out int adminId))
            //{
            //    adminId = 4;
            //}
            var task = await _db.CyTask.Where(x => x.IsVisible &&  x.ID == dto.ID && (x.AdminId==adminId || x.UserId== adminId)).FirstOrDefaultAsync();

            if (task == null) return NoContent();

            if (task.AdminId == adminId)
            {
                //task.AdminId = adminId;
                task.TaskState = dto.TaskState;
                task.Title = dto.Title;
                task.CompletionDate = dto.CompletionDate;
                task.Description = dto.Description;
                task.UserId = dto.UserId;
                task.Hidden = false;
                task.TaskKind = dto.TaskKind;
                task.Color = dto.Color;
                task.Important = dto.Important;
                task.Score = dto.Score;
            }
            else if(task.AdminId != adminId){
                task.TaskState = dto.TaskState;
            }


            ///مخفی کردن تسک در حالت کامل شده 
            if (dto.TaskState == TaskState.Completed)
            {
                task.Hidden= true;
            }

            await _db.SaveChangesAsync();

            return Ok(task);
        }

        [TypeAuthorize([UserType.SysAdmin])]
        [HttpDelete("deleteTask")]

        async public Task<ActionResult> deleteTask(int taskId)
        {

            var task = _db.CyTask.Where(x => x.IsVisible && x.ID == taskId).FirstOrDefault();
            if (task == null) return NoContent();

            task.IsVisible = false;

            await _db.SaveChangesAsync();

            return Ok(task);
        }



    }
}
