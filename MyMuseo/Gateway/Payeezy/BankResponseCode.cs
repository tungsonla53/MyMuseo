﻿using System.Collections.Generic;

namespace MyMuseo.Gateway
{
    public partial class PayeezyGateway : BaseGateway {
        public enum BankResponseCode {
            Unknown,
            NoAnswer,
            Approved,
            Validated,
            Verified,
            PreNoted,
            NoReasonToDecline,
            ReceivedAndStored,
            ProvidedAuth,
            RequestReceived,
            ApprovedForActivation,
            PreviouslyProcessedTransaction,
            BinAlert,
            ApprovedForPartial,
            ConditionalApproval,
            InvalidCardNumber,
            BadAmountNonnumericAmount,
            ZeroAmount,
            OtherError,
            BadTotalAuthAmount,
            InvalidSkuNumber,
            InvalidCreditPlan,
            InvalidStoreNumber,
            InvalidFieldData,
            MissingCompanionData,
            PercentsDoNotTotal100,
            PaymentsDoNotTotal100,
            InvalidDivisionNumber,
            DoesNotMatchMOP,
            DuplicateOrderNumber,
            FpoLocked,
            AuthRecycleHostSystemDown,
            FpoNotApproved,
            InvalidCurrency,
            InvalidMopForDivision,
            AuthAmountForDivision,
            IllegalAction,
            InvalidPurchaseLevel3,
            InvalidEncryptionFormat,
            MissingOrInvalidSecurePaymentData,
            MerchantNotMasterCardSecureCodeEnabled,
            CheckConversionDataError,
            BlanksNotPassedInReservedField,
            Invalid_MCC,
            InvalidStartDate,
            InvalidIssueNumber,
            InvalidTranType,
            MissingCustServicePhone,
            NotAuthorizedtoSendRecord,
            SoftAVS,
            AccountNotEligibleForDivisionsSetup,
            AuthorizationCodeResponseDateInvalid,
            PartialAuthorizationNotAllowedOrNotValid,
            DuplicateDepositTransaction,
            MissingQhpAmount,
            InvalidQhpAmount,
            TransactionNotSupported,
            IssuerUnavailable,
            CreditFloor,
            ProcessorDecline,
            NotOnFile,
            AlreadyReversed,
            AmountMismatch,
            AuthorizationNotFound,
            TransArmorServiceUnavailable,
            ExpiredLock,
            TransArmorInvalidTokenOrPAN,
            TransArmorInvalidResult,
            Call,
            DefaultCall,
            Pickup,
            Lost_Stolen,
            Fraud_Security_Violation,
            NegativeFile,
            ExcessivePinTry,
            OverTheLimit,
            OverLimitFrequency,
            InsufficientFunds,
            CardIsExpired,
            AlteredData,
            DoNotHonor,
            CVV2_VAK_Failure,
            DoNotHonorHighFraud,
            StopPaymentOrderOneTimeRecurringInstallment,
            RevocationOfAuthorizationForAllRecurringInstallments,
            RevocationOfAllAuthorizationsClosedAccount,
            AccountPreviouslyActivated,
            UnableToVoid,
            BlockActivationFailed,
            IssuanceDoesNotMeetMinimumAmount,
            NoOriginalAuthorizationFound,
            OutstandingAuthorizationFundsOnHold,
            ActivationAmountIncorrect,
            CvdValueFailure,
            MaximumRedemptionLimitMet,
            BadAmount,
            NewCardIssued,
            SuspectedFraud,
            RefundNotAllowed,
            InvalidInstitutionCode,
            InvalidInstitution,
            InvalidExpirationDate,
            InvalidTransactionType,
            InvalidAmount,
            BinBlock,
            FpoAccepted,
            MatchFailed,
            ValidationFailed,
            InvalidTransitRoutingNumber,
            TransitRoutingNumberUnknown,
            MissingName,
            InvalidAccountTypeFixPertainsToDepositTransactionsOnly,
            AccountClosed,
            SubscriberNumberDoesNotExist,
            SubscriberNumberNotActive,
            AchNonParticipant,
            DuplicateCheck,
            OriginalTransactionWasNotApproved,
            Rejected_Code_3_Risk,
            RefundOrPartialAmountIsGreaterThanOriginalSaleAmount,
            PositiveId,
            Restraint,
            InvalidSecurityCode,
            InvalidPin,
            NoAccount,
            InvalidMerchant,
            UnauthorizedUser,
            ProcessUnavailable,
            InvalidExpiration,
            InvalidEffective,
            AcquirerError
        }

        protected Dictionary<string, BankResponseCode> BankResponseCodeByString = new Dictionary<string, BankResponseCode>() {
            { "000", BankResponseCode.NoAnswer },
            { "100", BankResponseCode.Approved },
            { "101", BankResponseCode.Validated },
            { "102", BankResponseCode.Verified },
            { "103", BankResponseCode.PreNoted },
            { "104", BankResponseCode.NoReasonToDecline },
            { "105", BankResponseCode.ReceivedAndStored },
            { "106", BankResponseCode.ProvidedAuth },
            { "107", BankResponseCode.RequestReceived },
            { "108", BankResponseCode.ApprovedForActivation },
            { "109", BankResponseCode.PreviouslyProcessedTransaction },
            { "110", BankResponseCode.BinAlert },
            { "111", BankResponseCode.ApprovedForPartial },
            { "164", BankResponseCode.ConditionalApproval },
            { "201", BankResponseCode.InvalidCardNumber },
            { "202", BankResponseCode.BadAmountNonnumericAmount },
            { "203", BankResponseCode.ZeroAmount },
            { "204", BankResponseCode.OtherError },
            { "205", BankResponseCode.BadTotalAuthAmount },
            { "218", BankResponseCode.InvalidSkuNumber },
            { "219", BankResponseCode.InvalidCreditPlan },
            { "220", BankResponseCode.InvalidStoreNumber },
            { "225", BankResponseCode.InvalidFieldData },
            { "227", BankResponseCode.MissingCompanionData },
            { "229", BankResponseCode.PercentsDoNotTotal100 },
            { "230", BankResponseCode.PaymentsDoNotTotal100 },
            { "231", BankResponseCode.InvalidDivisionNumber },
            { "233", BankResponseCode.DoesNotMatchMOP },
            { "234", BankResponseCode.DuplicateOrderNumber },
            { "235", BankResponseCode.FpoLocked },
            { "236", BankResponseCode.AuthRecycleHostSystemDown },
            { "237", BankResponseCode.FpoNotApproved },
            { "238", BankResponseCode.InvalidCurrency },
            { "239", BankResponseCode.InvalidMopForDivision },
            { "240", BankResponseCode.AuthAmountForDivision },
            { "241", BankResponseCode.IllegalAction },
            { "243", BankResponseCode.InvalidPurchaseLevel3 },
            { "244", BankResponseCode.InvalidEncryptionFormat },
            { "245", BankResponseCode.MissingOrInvalidSecurePaymentData },
            { "246", BankResponseCode.MerchantNotMasterCardSecureCodeEnabled },
            { "247", BankResponseCode.CheckConversionDataError },
            { "248", BankResponseCode.BlanksNotPassedInReservedField },
            { "249", BankResponseCode.Invalid_MCC },
            { "251", BankResponseCode.InvalidStartDate },
            { "252", BankResponseCode.InvalidIssueNumber },
            { "253", BankResponseCode.InvalidTranType },
            { "257", BankResponseCode.MissingCustServicePhone },
            { "258", BankResponseCode.NotAuthorizedtoSendRecord },
            { "260", BankResponseCode.SoftAVS },
            { "261", BankResponseCode.AccountNotEligibleForDivisionsSetup },
            { "262", BankResponseCode.AuthorizationCodeResponseDateInvalid },
            { "263", BankResponseCode.PartialAuthorizationNotAllowedOrNotValid },
            { "264", BankResponseCode.DuplicateDepositTransaction },
            { "265", BankResponseCode.MissingQhpAmount },
            { "266", BankResponseCode.InvalidQhpAmount },
            { "274", BankResponseCode.TransactionNotSupported },
            { "301", BankResponseCode.IssuerUnavailable },
            { "302", BankResponseCode.CreditFloor },
            { "303", BankResponseCode.ProcessorDecline },
            { "304", BankResponseCode.NotOnFile },
            { "305", BankResponseCode.AlreadyReversed },
            { "306", BankResponseCode.AmountMismatch },
            { "307", BankResponseCode.AuthorizationNotFound },
            { "351", BankResponseCode.TransArmorServiceUnavailable },
            { "352", BankResponseCode.ExpiredLock },
            { "353", BankResponseCode.TransArmorInvalidTokenOrPAN },
            { "354", BankResponseCode.TransArmorInvalidResult },
            { "401", BankResponseCode.Call },
            { "402", BankResponseCode.DefaultCall },
            { "501", BankResponseCode.Pickup },
            { "502", BankResponseCode.Lost_Stolen },
            { "503", BankResponseCode.Fraud_Security_Violation },
            { "505", BankResponseCode.NegativeFile },
            { "508", BankResponseCode.ExcessivePinTry },
            { "509", BankResponseCode.OverTheLimit },
            { "510", BankResponseCode.OverLimitFrequency },
            { "519", BankResponseCode.NegativeFile },
            { "521", BankResponseCode.InsufficientFunds },
            { "522", BankResponseCode.CardIsExpired },
            { "524", BankResponseCode.AlteredData },
            { "530", BankResponseCode.DoNotHonor },
            { "531", BankResponseCode.CVV2_VAK_Failure },
            { "534", BankResponseCode.DoNotHonorHighFraud },
            { "570", BankResponseCode.StopPaymentOrderOneTimeRecurringInstallment },
            { "571", BankResponseCode.RevocationOfAuthorizationForAllRecurringInstallments },
            { "572", BankResponseCode.RevocationOfAllAuthorizationsClosedAccount },
            { "580", BankResponseCode.AccountPreviouslyActivated },
            { "581", BankResponseCode.UnableToVoid },
            { "582", BankResponseCode.BlockActivationFailed },
            { "583", BankResponseCode.BlockActivationFailed },
            { "584", BankResponseCode.IssuanceDoesNotMeetMinimumAmount },
            { "585", BankResponseCode.NoOriginalAuthorizationFound },
            { "586", BankResponseCode.OutstandingAuthorizationFundsOnHold },
            { "587", BankResponseCode.ActivationAmountIncorrect },
            { "588", BankResponseCode.BlockActivationFailed },
            { "589", BankResponseCode.CvdValueFailure },
            { "590", BankResponseCode.MaximumRedemptionLimitMet },
            { "591", BankResponseCode.InvalidCardNumber },
            { "592", BankResponseCode.BadAmount },
            { "594", BankResponseCode.OtherError },
            { "595", BankResponseCode.NewCardIssued },
            { "596", BankResponseCode.SuspectedFraud },
            { "599", BankResponseCode.RefundNotAllowed },
            { "602", BankResponseCode.InvalidInstitutionCode },
            { "603", BankResponseCode.InvalidInstitution },
            { "605", BankResponseCode.InvalidExpirationDate },
            { "606", BankResponseCode.InvalidTransactionType },
            { "607", BankResponseCode.InvalidAmount },
            { "610", BankResponseCode.BinBlock },
            { "704", BankResponseCode.FpoAccepted },
            { "740", BankResponseCode.MatchFailed },
            { "741", BankResponseCode.ValidationFailed },
            { "750", BankResponseCode.InvalidTransitRoutingNumber },
            { "751", BankResponseCode.TransitRoutingNumberUnknown },
            { "752", BankResponseCode.MissingName },
            { "753", BankResponseCode.InvalidAccountTypeFixPertainsToDepositTransactionsOnly },
            { "754", BankResponseCode.AccountClosed },
            { "755", BankResponseCode.SubscriberNumberDoesNotExist },
            { "758", BankResponseCode.SubscriberNumberNotActive },
            { "760", BankResponseCode.AchNonParticipant },
            { "776", BankResponseCode.DuplicateCheck },
            { "777", BankResponseCode.OriginalTransactionWasNotApproved },
            { "787", BankResponseCode.Rejected_Code_3_Risk },
            { "788", BankResponseCode.RefundOrPartialAmountIsGreaterThanOriginalSaleAmount },
            { "802", BankResponseCode.PositiveId },
            { "806", BankResponseCode.Restraint },
            { "811", BankResponseCode.InvalidSecurityCode },
            { "813", BankResponseCode.InvalidPin },
            { "825", BankResponseCode.NoAccount },
            { "833", BankResponseCode.InvalidMerchant },
            { "834", BankResponseCode.UnauthorizedUser },
            { "902", BankResponseCode.ProcessUnavailable },
            { "903", BankResponseCode.InvalidExpiration },
            { "904", BankResponseCode.InvalidEffective },
            { "997", BankResponseCode.AcquirerError }
        };
    }
}