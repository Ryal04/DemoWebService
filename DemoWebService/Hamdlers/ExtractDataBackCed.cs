using Amazon.Textract;
using Amazon.Textract.Model;
using DemoWebService.Domain;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DemoWebService.Hamdlers
{
    public class ExtractDataBackCed
    {

        public class Query : IRequest<ModelBackCed>
        {
            public MemoryStream cedBack { get; set; }
        }

        public class Handler : IRequestHandler<Query, ModelBackCed>
        {

            private readonly IAmazonTextract amazonTextract;

            public Handler(IAmazonTextract amazonTextract)
            {
                this.amazonTextract = amazonTextract;
            }

            public async Task<ModelBackCed> Handle(Query request, CancellationToken cancellationToken)
            {

                var cont = 0;
                var list = new List<String>();

                Document ImageTarget = new Document();
                ImageTarget.Bytes = request.cedBack;

                var response = await amazonTextract.DetectDocumentTextAsync(new DetectDocumentTextRequest()
                {
                    Document = ImageTarget,
                });

                foreach (var data in response.Blocks)
                {

                    if (data.BlockType.Value == "LINE" && data.Confidence >= 85)
                    {
                        cont++;

                        if (data.Text != "FECHA DE NACIMIENTO" && cont == 1)
                        {
                            list.Add(data.Text);
                        }

                        if(data.Text != "LUGAR DE NACIMIENTO"
                        && data.Text != "SEXO"
                        && data.Text != "G.S. RH"
                        && data.Text != "ESTATURA"
                        && cont > 1 && cont < 13)
                        {
                           list.Add(data.Text);
                        }
                    }
                }

                //Split
                string[] fechanacimiento = list[0].Split(' ');
                if (fechanacimiento.Length > 1)
                {
                    list[0] = fechanacimiento[3];
                }

                //Split
                string[] DepartamentoNacimiento = list[2].Split('(',')');
                if (DepartamentoNacimiento.Length > 1)
                {
                    list[2] = DepartamentoNacimiento[1];
                }

                //Split
                string[] fechaylugarexp = list[6].Split(' ');
                if (fechaylugarexp.Length > 1)
                {
                    list[6] = fechaylugarexp[1];
                    
                    if (list.Count > 7)
                    {
                        list[7] = fechaylugarexp[0];
                    }else {
                        list.Add(fechaylugarexp[0]);
                    }
                }

                //New obj ModelBackCed return
                return new ModelBackCed()
                {
                    fechaNacimiento = list[0],
                    ciudadNacimiento = list[1],
                    departamentoNacimiento = list[2],
                    estatura = list[3],
                    rh = list[4],
                    sexo = list[5],
                    lugarDeExpedicion = list[6],
                    fechaDeExpedicion = list[7]
                };
            }
        }
    }
}
