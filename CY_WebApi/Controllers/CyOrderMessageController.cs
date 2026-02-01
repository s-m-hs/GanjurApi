using AutoMapper;
using CY_DM;
using CY_BM;
using CY_WebApi.DataAccess;
using CY_WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using CY_WebApi.Services;
using Microsoft.Identity.Client;
using System.Security.Claims;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CY_WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyOrderMessageController : ControllerBase
    {
       
    }
}