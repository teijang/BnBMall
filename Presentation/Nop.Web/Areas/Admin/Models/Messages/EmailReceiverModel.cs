using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Messages;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Nop.Web.Areas.Admin.Models.Messages
{
    public partial class EmailReceiverListModel : BasePagedListModel<EmailReceiverModel>
    {
    }

    public class EmailReceiverModel: BaseNopEntityModel
    {
        public EmailReceiverModel() {
            Permissions = new List<int>();
            PermissionList = new List<SelectListItem>();
            PermissionList = Enum.GetValues(typeof(EmailReceiverPermission)).Cast<EmailReceiverPermission>().Select(x => {
                return new SelectListItem {
                    Text = x.ToString().Replace("_"," "),
                    Value = ((int)x).ToString()
                };
            }).ToList();
        }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Email")]
        public string EmailAddress { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Permissions")]
        public string PermissionText { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Permissions")]
        public IList<int> Permissions { get; set; }

        public IList<SelectListItem> PermissionList { get; set; }
    }
}
