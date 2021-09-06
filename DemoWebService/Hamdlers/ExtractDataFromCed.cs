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
    public class ExtractDataFromCed
    {
        public class Query : IRequest<ModelFromCed>
        {
            public MemoryStream cedFrom { get; set; }
        }

        public class Handler : IRequestHandler<Query, ModelFromCed>
        {

            private readonly IAmazonTextract amazonTextract;

            public Handler(IAmazonTextract amazonTextract)
            {
                this.amazonTextract = amazonTextract;
            }

            public async Task<ModelFromCed> Handle(Query request, CancellationToken cancellationToken)
            {

                var cont = 0;
                var list = new List<String>();
                Document ImageTarget = new Document();

                ImageTarget.Bytes = request.cedFrom;

                var response = await amazonTextract.DetectDocumentTextAsync(new DetectDocumentTextRequest()
                {
                    Document = ImageTarget,
                });


                foreach (var data in response.Blocks)
                {
                    if (data.BlockType.Value == "LINE" && data.Confidence >= 85)
                    {
                        cont++;

                        if (cont > 2 && cont != 4)
                        {
                            if (data.Text != "NOMBRES" && data.Text != "APELLIDOS" && data.Text != "FIRMA" && data.Text != "NUMERO")
                            {
                                list.Add(data.Text);
                            }
                        }

                        if (cont == 4)
                        {
                            if (data.Text != "NÚMERO" && data.Text != "NUMERO")
                            {
                                string[] split = data.Text.Split(' ');

                                if (split.Length != 1)
                                {
                                    list.Add(split[1]);
                                }
                                else
                                {
                                    list.Add(data.Text);
                                }
                            }
                        }
                    }
                }

                return new ModelFromCed()
                {
                    tipoIdentificacion = list[0],
                    numIdentificacion = list[1],
                    apellidos = list[2],
                    nombres = list[3],

                };

            }
        }
    }
}
