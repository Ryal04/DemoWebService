using Amazon.Rekognition;
using Amazon.Rekognition.Model;
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
    public class CompareFaces
    {

        public class Query : IRequest<ModelCompareFace>
        {
            public MemoryStream cedFrom { get; set; }
            public MemoryStream retrat { get; set; }
        }

        public class Handler : IRequestHandler<Query, ModelCompareFace>
        {
            private readonly IAmazonRekognition rekognition;

            public Handler(IAmazonRekognition rekognition)
            {
                this.rekognition = rekognition;
            }

            public async Task<ModelCompareFace> Handle(Query request, CancellationToken cancellationToken)
            {
                try
                {
                    // Declaracion de variables
                    var similarity = 0F;
                    var similarityThreshold = 90F;
                    Image CedFrom = new Image();
                    Image Retrat = new Image();

                    CedFrom.Bytes = request.cedFrom;
                    Retrat.Bytes = request.retrat;

                    var response = await rekognition.CompareFacesAsync(new CompareFacesRequest()
                    {
                        //Imagen Byte 1
                        SourceImage = CedFrom,
                        //Imagen Byte 2
                        TargetImage = Retrat,
                        // Umbral de Similaridad
                        SimilarityThreshold = similarityThreshold,

                    });

                    foreach (var data in response.FaceMatches)
                    {
                        similarity = data.Similarity;
                    }

                    return new ModelCompareFace()
                    {
                        similarity = similarity
                    };

                }
                catch (Exception e)
                {
                    Console.WriteLine("--> Error en Comparacion de Rostros");
                    Console.WriteLine("--> " + e);
                    //New Null Obj ModelBackCed Return 
                    return null;
                }
                
            }
        }
    }
}
