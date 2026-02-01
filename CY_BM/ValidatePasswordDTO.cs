using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CY_BM
{
    /// <summary>
    /// برای تایید کد پیامک شده
    /// </summary>
    public class ValidatePasswordDTO
    {
        public int ValadationCode {  get; set; }
        public required string Username { get; set; }
    }
}
