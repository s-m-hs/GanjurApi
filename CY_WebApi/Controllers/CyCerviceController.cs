using System.Security.Claims;
using AutoMapper;
using ClosedXML.Excel;
using CY_BM;
using CY_DM;
using CY_WebApi.DataAccess;
using CY_WebApi.Models;
using CY_WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyCerviceController : ControllerBase
    {
        private readonly CyContext _db;
        private readonly IMapper _mapper;

        public CyCerviceController(CyContext db, IMapper mapper)
        {
            _mapper = mapper;
            _db = db;
        }

        [HttpPost("addService")]
        async public Task<ActionResult> addService([FromBody] ServiceDTO dto) { 
        
            var newService=_mapper.Map<CyService>(dto);

            _db.CyService.Add(newService);
            await _db.SaveChangesAsync();

            return Ok(newService.ID);

        }

        [HttpGet("getService")]
        async public Task<ActionResult> getService(int id) { 
        var currentService=await _db.CyService.Where(x=>x.IsVisible && x.ID == id).FirstOrDefaultAsync();
            if (currentService == null) return BadRequest(new { msg = "رسید پیدا نشد" });

           var dto=_mapper.Map<ServiceDTO>(currentService);


            return Ok(dto);
        }


        [HttpGet("getAllServicesByType")]
        async public Task<ActionResult> getServices(GuaranteeType type)
        {
            var servicesList =await _db.CyService.Where(x => x.IsVisible && x.Type == type).ToListAsync();

            var dto = _mapper.Map<List<ServiceDTO>>(servicesList);

            dto.Reverse();

            return Ok(dto);
        }

        /// <summary>
        /// شماره کاربران خدمات در قالب فایل اکسل
        /// </summary>
        /// <returns></returns>
        [HttpGet("ExportServicesExcel")]
        public async Task<IActionResult> ExportServicesExcel()
        {
            var services = await _db.CyService
                .Select(x => new
                {
                    x.Mobile,
                    x.CustomerName,
                    x.CreateDate
                })
                .ToListAsync();

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Services");

            // Header
            worksheet.Cell(1, 1).Value = "شماره موبایل";
            worksheet.Cell(1, 2).Value = "نام مشتری";
            worksheet.Cell(1, 3).Value = "تاریخ ثبت";

            // Data
            for (int i = 0; i < services.Count; i++)
            {
                var row = i + 2;

                worksheet.Cell(row, 1).Value = services[i].Mobile;
                worksheet.Cell(row, 2).Value = services[i].CustomerName;

                    worksheet.Cell(row, 3).Value = services[i].CreateDate;
                    worksheet.Cell(row, 3).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
                
            }

            // تنظیم عرض ستون‌ها
            worksheet.Column(1).Width = 20;
            worksheet.Column(2).Width = 30;
            worksheet.Column(3).Width = 22;

            // استایل Header
            var header = worksheet.Range("A1:C1");
            header.Style.Font.Bold = true;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // فیلتر
            worksheet.RangeUsed().SetAutoFilter();

            // Freeze Header
            worksheet.SheetView.FreezeRows(1);

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            var fileName = $"Services_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
    }
}