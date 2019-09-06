﻿using System.Collections.Generic;

namespace MyMuseo.Gateway
{
    public partial class PayeezyGateway : BaseGateway {
        protected Dictionary<CreditCardType, string> CardTypeToString = new Dictionary<CreditCardType, string>() {
            { CreditCardType.AmericanExpress, "American Express" },
            { CreditCardType.Visa, "Visa" },
            { CreditCardType.MasterCard, "Mastercard" },
            { CreditCardType.Discover, "Discover" }
        };
    }
}
