using FluentValidation;
using FluentValidation.Results;
using Paycor.Neo.Rest.Errors.V1;

namespace Paycor.Storage.Host.WebApi.Models.V1
{
    public class DocumentRequestValidator : AbstractValidator<DocumentRequest>
    {
        // rjg: consider moving error codes to enum class
        public const string DOCUMENT_NOT_FOUND_ERROR_CODE = "STO10000";
        public const string INVALID_DOCUMENT_REQUEST_ERROR_CODE = "STO10001";
        public const string INVALID_DOC_ID_ERROR_CODE = "STO10011";
        public const string INVALID_NAME_ERROR_CODE = "STO10012";
        public const string INVALID_OWNER_ID_ERROR_CODE = "STO10013";
        public const string INVALID_DOC_TYPE_ERROR_CODE = "STO10014";

        private const string INVALID_DOCUMENT_REQUEST_MSG = "'DocumentRequest' is either missing or malformed";
        private const string DOCUMENT_NOT_FOUND_MSG = "Document not found";
        private const string INVALID_DOCUMENT_ID_MSG = "Invalid document ID";

        private static readonly ValidationState _validationStateDocNotFound = new ValidationState(DOCUMENT_NOT_FOUND_ERROR_CODE);
        private static readonly ValidationState _validationStateInvalidDoc = new ValidationState(INVALID_DOCUMENT_REQUEST_ERROR_CODE);
        private static readonly ValidationState _validationStateInvalidDocId = new ValidationState(INVALID_DOC_ID_ERROR_CODE, ValidationState.URL_LOCATION);
        private static readonly ValidationState _validationStateName = new ValidationState(INVALID_NAME_ERROR_CODE);
        private static readonly ValidationState _validationStateOwnerId = new ValidationState(INVALID_OWNER_ID_ERROR_CODE);
        private static readonly ValidationState _validationStateDocTypeCode = new ValidationState(INVALID_DOC_TYPE_ERROR_CODE);

        public DocumentRequestValidator()
        {
            RuleFor(doc => doc.Name).NotEmpty().WithState(doc => _validationStateName);
            RuleFor(doc => doc.OwnerId).NotEmpty().WithState(doc => _validationStateOwnerId);
            RuleFor(doc => doc.DocumentType).NotEmpty().WithState(doc => _validationStateDocTypeCode);
        }

        public override ValidationResult Validate(DocumentRequest instance)
        {
            return instance == null
                ? CreateValidationFailure("DocumentRequest", INVALID_DOCUMENT_REQUEST_MSG, null, _validationStateInvalidDoc) 
                : base.Validate(instance);
        }

        public ValidationResult HandleInvalidDocId(string id)
        {
            return CreateValidationFailure("id", INVALID_DOCUMENT_ID_MSG, id, _validationStateInvalidDocId);
        }

        public ValidationResult HandleDocumentNotFound(string id)
        {
            return CreateValidationFailure("id", DOCUMENT_NOT_FOUND_MSG, id, _validationStateDocNotFound);
        }

        private ValidationResult CreateValidationFailure(string property, string message, object attemptedValue = null, object state = null)
        {
            var failure = new ValidationFailure(property, message, attemptedValue)
            {
                CustomState = state
            };
            return new ValidationResult(new [] { failure });
        }

    }
}