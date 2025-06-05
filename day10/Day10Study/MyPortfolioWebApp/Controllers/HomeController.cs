using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolioWebApp.Models;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;

namespace MyPortfolioWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context; // DB����

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> About()
        {
            // ���� HTML�� DB �����ͷ� ����ó��
            // DB���� �����͸� �ҷ��� �� About, Skill ��ü�� ������ ��Ƽ� ��� �Ѱ���
            var skillCount = _context.Skill.Count();
            var skill = await _context.Skill.ToListAsync();
            // FirstAsync�� �����Ͱ� ������ ���ܹ߻�. FirstOrDefaultAsync �����Ͱ� ������ �ΰ�
            var about = await _context.About.FirstOrDefaultAsync(); 

            ViewBag.SkillCount = skillCount; // ex. 7�� �Ѿ
            ViewBag.ColNum = (skillCount / 2) + (skillCount % 2); // 3(7/2) + 1(7%2)

            var model = new AboutModel();
            model.About = about;
            model.Skill = skill;

            return View(model);
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactModel model)
        {
            if (ModelState.IsValid) // Model에 들어간 네개 값이 제대로 들어갔으면            
            {
                try
                {
                    var smtpClient = new SmtpClient("smtp.gmail.com")   // Gmail을 사용하면
                    {
                        Port = 587, // 메일 SMPT 서비스포트 변경필요
                        Credentials = new NetworkCredential("goosalsok@naver.com", "비밀번호 노출"),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(model.Email),    // 문의하기에 작성한 메일주소
                        Subject = model.Subject ?? "[제목없음]",
                        Body = $"보낸 사람 : {model.Name} ({model.Email})\n\n메시지 : {model.Message}",
                        IsBodyHtml = false, // 메일 본문에 HTML태그를 사용여부
                    };

                    mailMessage.To.Add("goosalsok@naver.com");  // 실제 받을 메일 주소

                    await smtpClient.SendMailAsync(mailMessage); // 위 생성도니 메일객체를 전송!
                    ViewBag.Success = true;
                }
                catch (Exception ex)
                {
                    ViewBag.Success = false;
                    ViewBag.Error = $"메일전송 실패! {ex.Message}";
                }                               
            }
            
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
