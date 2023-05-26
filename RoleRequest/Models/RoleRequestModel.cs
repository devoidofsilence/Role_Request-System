using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace RoleRequest.Models
{
    public class RoleRequestModel
    {
        public int RoleRequestId { get; set; }

        //[Required(ErrorMessage = "Please provide request date")]
        public string RoleRequestDate { get; set; }
        [Required(ErrorMessage = "Please provide employee Id.")]
        public string EmpSamAccountName { get; set; }
        [Required(ErrorMessage = "Please enter employee's full name.")]
        public string EmployeeFullName { get; set; }
        [Required(ErrorMessage = "Please provide the Department.")]
        public string DepartmentId { get; set; }

        public string DepartmentName { get; set; }
        [Required(ErrorMessage = "Please provide the Branch.")]
        public string BranchLocationId { get; set; }

        public string BranchName { get; set; }

        public string CorporateTitleId { get; set; }

        public string CorporateTitle { get; set; }

        //public string FunctionalTitleId { get; set; }

        public string FunctionalTitle { get; set; }

        //public string FlexcubeUBSAuthorizationRemarks { get; set; }

        public decimal InputLimitAmt { get; set; }

        public decimal AuthorizationLimitAmt { get; set; }

        //public string EdmsAuthorizationRemarks { get; set; }

        public string AdditionalRequest { get; set; }

        public bool IsRecommended { get; set; }

        public int RecommendedById { get; set; }
        [Required(ErrorMessage = "Please specify a recommender.")]
        public string RecommendationByEmpName { get; set; }
        [Required(ErrorMessage = "Recommender username not specified.")]
        public string RecommendationBySAM { get; set; }
        [Required(ErrorMessage = "Please provide remarks.")]
        [DataType(DataType.MultilineText)]
        public string Remarks { get; set; }

        public bool IsApproved { get; set; }

        public int ApprovedById { get; set; }
        [Required(ErrorMessage = "Please specify an approver.")]
        public string ApprovalByEmpName { get; set; }
        [Required(ErrorMessage = "Approver username not specified.")]
        public string ApprovalBySAM { get; set; }
        //[Required(ErrorMessage = "Please provide approval remarks.")]
        //[DataType(DataType.MultilineText)]
        //public string ApprovalRemarks { get; set; }

        public string RequestStatus { get; set; }

        public bool AccessGiven { get; set; }

        public string AccessGiver { get; set; }

        public string AccessProcessStatus { get; set; }

        public List<PrimaryRoleModel> lstPrimaryRoleModel { get; set; }
        public List<PrimaryRoleModel> lstPrimaryRoleModelLatestSavedRequest { get; set; }
        public List<AppRolesModel> lstAppRolesModelLatestSavedRequest { get; set; }
        public List<AppAccessLevelModel> lstAccessLevelModelLatestSavedRequest { get; set; }

        public AppDomainModel secondaryRoleDomain { get; set; }

        public AppDomainModel flexcubeRoleDomain { get; set; }

        public AppDomainModel edmsRoleDomain { get; set; }

        public bool editMode { get; set; }
        public bool viewOnlyMode { get; set; }
        public bool recommendMode { get; set; }
        public bool approveMode { get; set; }
        public bool revokeMode { get; set; }
        public bool rejectionFlag { get; set; }
        public bool takenBackFlag { get; set; }

        public List<RemarksReport> remarksList { get; set; }

        //Lately added fields
        [Required(ErrorMessage = "Please provide HRIS ID.")]
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public string HRIS_ID { get; set; }
        public string FLEX_ID { get; set; }
        [Required(ErrorMessage = "Please provide Email ID.")]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}", ErrorMessage = "Please enter correct email")]
        public string EMAIL_ID { get; set; }
        [Required(ErrorMessage = "Please provide Mobile Number.")]
        [Range(1, 10000000000, ErrorMessage = "Please provide valid Mobile Number")]
        public string MOBILE_NUMBER { get; set; }
    }
}