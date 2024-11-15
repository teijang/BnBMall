using Nop.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Core.Domain.Messages
{
    public enum EmailReceiverPermission
    {
        Administrators = 1,
        Shipping_Manager = 2,
        QA_Manager = 3,
        CS_Manager = 4
    }

    public class EmailReceivers : BaseEntity
    {
        /// <summary>
        /// Gets or sets value of email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets value of permission, comma seprated
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// Email receiver permissions
        /// </summary>
        public virtual IList<EmailReceiverPermission> EmailReceiverPemissions {
            get {
                if(!string.IsNullOrEmpty(Permission)) {
                    var permissions = Permission.Split(new string[] { "," }, System.StringSplitOptions.RemoveEmptyEntries).Select(x => Convert.ToInt32(x.Trim()));
                    return permissions.Select(x => (EmailReceiverPermission)x).ToList();
                } else {
                    return new List<EmailReceiverPermission>();
                }
            }
            set {
                Permission = string.Join(',', value.Select(x => (int)x));
            }
        }
    }
}
