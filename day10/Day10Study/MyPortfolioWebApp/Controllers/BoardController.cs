﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyPortfolioWebApp.Models;

namespace MyPortfolioWebApp.Controllers
{
    public class BoardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BoardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Board
        public async Task<IActionResult> Index(int page = 1, string search = "")
        {
            // 뷰쪽에 보내고 싶은 데이터
            ViewData["Title"] = "서버에서 변경가능";

            var totalCount = _context.Board.Where(n => EF.Functions.Like(n.Title, $"%{search}%")).Count(); // HACK! 위 구문이 오류발생
            var countList = 10; // 한페이지에 기본 뉴스갯수 10개
            var totalPage = totalCount / countList; // 한페이지당 개수로 나누면 전체페이지 수
            // HACK : 게시판페이지 중요로직. 남는 데이터도 한페이지를 차지해야 함
            if (totalCount % countList > 0) totalPage++;  // 남은 게시글이 있으면 페이지수 증가            
            if (totalPage < page) page = totalPage;
            // 마지막 페이지 구하기
            var countPage = 10; // 페이지를 표시할 최대페이지개수, 10개
            var startPage = ((page - 1) / countPage) * countPage + 1;
            var endPage = startPage + countPage - 1;
            // HACK : 나타낼 페이수가 10이 안되면 페이지수 조정.
            // 마지막페이지까지 글이 12개면  1, 2 페이지만 표시
            if (totalPage < endPage) endPage = totalPage;

            // 저장프로시저에 보낼 rowNum값, 시작번호랑 끝번호
            var startCount = ((page - 1) * countPage) + 1; // 2페이지의 경우 11
            var endCount = startCount + countList - 1; // 2페이지의 경우 20

            // View로 넘기는 데이터, 페이징 숫자컨트롤 사용
            ViewBag.StartPage = startPage;
            ViewBag.EndPage = endPage;
            ViewBag.Page = page;
            ViewBag.TotalPage = totalPage;
            ViewBag.Search = search; // 검색어

            // 저장프로시저 호출
            var board = await _context.Board
                .FromSql($"CALL Board_PagingBoard({startCount}, {endCount}, {search})")
                .ToListAsync();
            return View(board);
        }

        // GET: Board/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var board = await _context.Board
                .FirstOrDefaultAsync(m => m.Id == id);
            if (board == null)
            {
                return NotFound();
            }

            // 조회수 증가 로직
            board.ReadCount++;
            _context.Board.Update(board);
            await _context.SaveChangesAsync();

            return View(board);
        }

        // GET: Board/Create
        public IActionResult Create()
        {
            var board = new Board
            {
                PostDate = DateTime.Now,
                ReadCount = 0,
            };
            return View();
        }

        // POST: Board/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email,Writer,Title,Contents")] Board board)
        {
            if (ModelState.IsValid)
            {
                board.PostDate = DateTime.Now; // 서버에서 현재 시간 설정
                board.ReadCount = 0;           // 처음에는 조회수 0

                _context.Add(board);
                await _context.SaveChangesAsync();

                TempData["success"] = "게시글이 성공적으로 등록되었습니다!";
                return RedirectToAction(nameof(Index));
            }

            return View(board); // 유효성 검사 실패 시 다시 폼 표시
        }

        // GET: Board/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var board = await _context.Board.FindAsync(id);
            if (board == null)
            {
                return NotFound();
            }
            return View(board);
        }

        // POST: Board/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Contents")] Board board)
        {
            if (id != board.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // 방식2: 기존 데이터를 찾아 수정하는 방식
                    var existingBoard = await _context.Board.FindAsync(id);
                    if (existingBoard == null)
                    {
                        return NotFound();
                    }

                    // 수정할 필드만 업데이트
                    existingBoard.Title = board.Title;
                    existingBoard.Contents = board.Contents;

                    // UPDATE Boards SET ...
                    await _context.SaveChangesAsync();

                    TempData["success"] = "게시글 수정 성공!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BoardExists(board.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            return View(board);
        }


        // GET: Board/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var board = await _context.Board
                .FirstOrDefaultAsync(m => m.Id == id);
            if (board == null)
            {
                return NotFound();
            }

            return View(board);
        }

        // POST: Board/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var board = await _context.Board.FindAsync(id);
            if (board != null)
            {
                // DELETE FROM Boards WHERE Id = @id
                _context.Board.Remove(board);
            }

            // COMMIT
            await _context.SaveChangesAsync();

            TempData["success"] = "게시글 삭제 성공!";
            return RedirectToAction(nameof(Index));
        }

        private bool BoardExists(int id)
        {
            return _context.Board.Any(e => e.Id == id);
        }
    }
}
