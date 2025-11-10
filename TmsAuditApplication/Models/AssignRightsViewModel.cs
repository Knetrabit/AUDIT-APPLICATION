namespace VrsAuditApplication.Models
{
    public class AssignRightsViewModel
    {
        public List<JobPosition> JobPositions { get; set; }
        public List<UserRight> AssignedRights { get; set; }
        public List<UserRight> UnassignedRights { get; set; }
    }

}
