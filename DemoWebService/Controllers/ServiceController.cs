using DemoWebService.Domain;
using DemoWebService.Hamdlers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DemoWebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {

        private IMediator _mediator;
        protected IMediator Mediator =>
                _mediator ?? (
                    _mediator = HttpContext.RequestServices.GetService<IMediator>()
                );

        [HttpPost]
        public async Task<ActionResult<ModelFromCed>> Consulta([FromForm] IFormFile CedFrontal, IFormFile CedDorsal, IFormFile Foto)
        {
            MemoryStream cedfrom, cedback, foto;
            var Response1 = new ModelFromCed();
            var Response2 = new ModelBackCed();
            var Response3 = new ModelCompareFace();

            if (CedFrontal != null && CedDorsal != null && Foto != null)
            {

                //Convercion Imagen a Bytes
                using (var ms = new MemoryStream())
                {
                    CedFrontal.CopyTo(ms);
                    cedfrom = new MemoryStream(ms.ToArray());
                    // act on the Base64 data
                }

                //Convercion Imagen a Bytes
                using (var ms = new MemoryStream())
                {
                    CedDorsal.CopyTo(ms);
                    cedback = new MemoryStream(ms.ToArray());
                    // act on the Base64 data
                }

                //Convercion Imagen a Bytes
                using (var ms = new MemoryStream())
                {
                    Foto.CopyTo(ms);
                    foto = new MemoryStream(ms.ToArray());
                    // act on the Base64 data
                }

                
                Response1 = await Mediator.Send(new ExtractDataFromCed.Query { cedFrom = cedfrom });
                Response2 = await Mediator.Send(new ExtractDataBackCed.Query { cedBack = cedback });
                
                if (Response1 != null && Response2 != null)
                {
                    Response3 = await Mediator.Send(new CompareFaces.Query { cedFrom = cedfrom, retrat = foto });
                }

                if (Response1 != null && Response2 != null && Response3 != null) {
                    return Ok(new ManagerClass()
                    {
                        ModelFromCed = Response1,
                        ModelBackCed = Response2,
                        ModelCompareFace = Response3
                    });
                }

                return BadRequest("id is required");
            }

            return BadRequest("Images Is Required");

        }

    }
}
