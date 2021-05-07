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

            var translatedText = await TranslateTextAsync(textToTranslate, targetLanguage);

            charactersCount += textToTranslate.Length;

            await _mediator.Send(new UpdateConfigurationValueCommand("FeatureTranslateCharactersCount", charactersCount.ToString()));

            var result = $"@{userName} says: \"{translatedText}\"";

            return result;
        }

        private async Task<string> TranslateTextAsync(string inputText, string targetLanguage)
        {
            var translatedText = string.Empty;

            var jsonCredentials = await _mediator.Send(new GetConfigurationQuery("FeatureTranslateKeys"));
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
