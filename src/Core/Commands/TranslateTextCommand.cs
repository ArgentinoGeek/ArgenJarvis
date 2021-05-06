using Core.Configuration.Commands;
using Core.Configuration.Queries;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Translation.V2;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Commands
{
    public class TranslateTextCommand : ITwitchCommand
    {
        private readonly IMediator _mediator;

        public TranslateTextCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<string> Execute(string commandName, string userName, IList<string> arguments, string argumentsAsString)
        {
            const int maxCharactersAllowed = 490000;

            var charactersCount = int.Parse(await _mediator.Send(new GetConfigurationQuery("FeatureTranslateCharactersCount")));
            var featureIsEnabled = bool.Parse(await _mediator.Send(new GetConfigurationQuery("FeatureTranslateEnabled")));

            if (!featureIsEnabled
                || arguments.Count < 1
                || charactersCount >= maxCharactersAllowed)
            {
                return string.Empty;
            }

            var targetLanguage = arguments.FirstOrDefault();

            var textToTranslate = argumentsAsString;
            textToTranslate = textToTranslate.Remove(textToTranslate.IndexOf(targetLanguage), targetLanguage.Length).Trim();

            if (string.IsNullOrWhiteSpace(textToTranslate))
            {
                return string.Empty;
            }

            var translatedText = TranslateText(textToTranslate, targetLanguage);

            charactersCount += textToTranslate.Length;

            await _mediator.Send(new UpdateConfigurationValueCommand("FeatureTranslateCharactersCount", charactersCount.ToString()));

            var result = $"@{userName} says: \"{translatedText}\"";

            return result;
        }

        private string TranslateText(string inputText, string targetLanguage)
        {
            var translatedText = string.Empty;

            var jsonCredentials = "{\"type\":\"service_account\",\"project_id\":\"argenjarvis\",\"private_key_id\":\"5446afaa7513f2fd6e54b37afc7d2b9bea8b09ae\",\"private_key\":\"-----BEGIN PRIVATE KEY-----\\nMIIEvAIBADANBgkqhkiG9w0BAQEFAASCBKYwggSiAgEAAoIBAQC/v9FqlPDCdvGE\\nu8swQs9bv+XAlb/lNJx15GHFtDUOordKZgqc7klmTVNxR7ZtAnNBJhfpTLVSxLzv\\nnZi8y4o75e6N5xvW6zWWOJ4K+skk9viVZNi+FdwiwlyochIzpq2YOCKvJS6KaVpA\\nZHqh1ITy0UrvF7UgPKM1kOMhIZEIzoMPzMTzRG535zr+4qXYw6OG+vQG36igf0c3\\n+BzUnnlndMbMJOfiDQfDCGCroETdqZ7wV9w2g8eF3YRiP0/qvc2CuWd3DmjRdrIw\\njCLCcmhEE8lN/Mz1Y0SNmgzWZ/5Nk4kPNQry8ZYdJP5AMZ8PlNQbrNMl8Psih0nw\\n2Qz/y8G3AgMBAAECggEAXq0wkoad5fbiw54R8m9agTvcDl4iVOLISjGM0RhshiBz\\njemaXegOF9QgWjOFcIegLs/HSKtWcFqxy0IuUzHQvEiIpEozZcfXOqgT6Onw/S5w\\niRs1JY4XlpYPOCCyugwXdKxq+73JIBBqvpaeMl+DkX0jmbBD/3nRQDac3xP6z7SD\\nzVfrVG5ymiZ+XIjxkt36rscYxuh1F+scZMHWGvSBpOn283dO1E4mm3IzNy6iGQPw\\nBsBE/t+6z4ten9jxln3S8nDxUvgJZJUSiHjyir1noCQMyKrNjhO9BK0zpks+oRDi\\nJGVnVlopqmsk5OSL6rnHkKfQSP8qpOCuUXpA9DtZOQKBgQD3BdG+bZkdvYkpsHMj\\nv3Wj8Ax/Xtv33dSYw66FvQsjMtUpWn2U1Vjm+5H1Up9yEZvuCpGPV4S6vtAmkR/9\\nlvtYx9oqweOiRbp+KZAcB0oY9SO+jrMAxyEM10uHpqGj4tvGRBFnhXO0JlToTeoT\\neQ89yak+DFvbVwzIyMTh5/I0mwKBgQDGt8Ldup71LKD6FCivlQQwpi2dXBCJz3Uk\\nwpNI1/bbABqUoJ7hepVEV+haEwuWooTnvaN2Dk7GJtUNii1+vtG4oU3CVE6OJksi\\ngencqVuKRkL334uyTdB2CJ0Ti6oujfFKMz80Cg8qJk44XCsCyt9480wtLecmnS3q\\nCOFVzVjjFQKBgHYSDP/laONjPdV0ZoR2ticmzQJwd9mQ6VYfaiK4ikNHv8VlYFMs\\narL1Gf3VgSrHqe1slcia/3E3VUyp302ZxgfYdrorNL/SbmJxQVV0UoF9YplpobvE\\nclKt8YckKWypOX0Z+euuSPCZxcnHvBOUsKlK458pzxoXEKBD/n8wG3/zAoGAGtf1\\n/efY0zvNHxscB+P0ZfH81XX7UfJxW57hMITj9t+Dt7Ie5Eyf31SUsZ5DAX1AwOFQ\\nNIFoSMI0I5kX3jg+slcv3uFunyszGR09jy5djEdDgqJzB4oVb+xn85z5R0KrZp+X\\nmRsGw2XbSSfCM6VeHMOY9b1urTusWSIL9oA/0qUCgYBAF4HlAwgiw4v27Q16nohZ\\nO49dm0s7qV9fomycG7GeWd2Pzk9WuoTh298NrhXzixx3xc1QSR2c5EH+mHJIIKRd\\nblwHFflf8Z6YH03WL4W8yBMwK2/HS5CmxTqcjU6ZNYtyg/2kmxrrFFmLWivFGir+\\nSd4/74sE1yuf9mQ+nlqKQg==\\n-----END PRIVATE KEY-----\\n\",\"client_email\":\"argenjarvis@argenjarvis.iam.gserviceaccount.com\",\"client_id\":\"109007526099783437688\",\"auth_uri\":\"https://accounts.google.com/o/oauth2/auth\",\"token_uri\":\"https://oauth2.googleapis.com/token\",\"auth_provider_x509_cert_url\":\"https://www.googleapis.com/oauth2/v1/certs\",\"client_x509_cert_url\":\"https://www.googleapis.com/robot/v1/metadata/x509/argenjarvis%40argenjarvis.iam.gserviceaccount.com\"}";
            var credentials = GoogleCredential.FromJson(jsonCredentials);
            using (var client = TranslationClient.Create(credentials))
            {
                var result = client.TranslateText(inputText, targetLanguage);

                translatedText = result.TranslatedText;
            }

            return translatedText;
        }
    }
}
