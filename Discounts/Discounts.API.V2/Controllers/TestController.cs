// Copyright (C) TBC Bank. All Rights Reserved.
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.V2.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")] //chahardcodeb
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                Version = "2.0",
                Message = "Es Aris Chemi Proektis meore versia romelze mushaobac" +
                " gagrdzeldeba sertifikatis gadmocemis Shemdeg",
                Status = "Faster, Better, Stronger, Drunker"
            });
        }
    }
}
