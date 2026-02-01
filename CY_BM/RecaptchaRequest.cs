using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    /// <summary>
    /// ورودی تایید کد کپچا
    /// </summary>
    public class RecaptchaRequest
    {
        public required string Token { get; set; }
        public required string Action { get; set; }
    }
}
