namespace Digitization.Services
{
    public class AuthService
    {
        private readonly ApplicationDBContext _context;

        public AuthService(ApplicationDBContext context)
        {
            _context = context;
        }
    }
}
