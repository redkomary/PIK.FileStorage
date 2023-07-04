using Microsoft.AspNetCore.Mvc;
using PIK.FileStorage.Models;

using IOFile = System.IO.File;

namespace PIK.FileStorage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
	private readonly string _fileStorageDirectory;


	public FileController(IConfiguration configuration)
	{
		_fileStorageDirectory = configuration["FileStoragePath"];
	}


	/// <summary>
	/// Download file by its name.
	/// </summary>
	/// <remarks>
	/// Sample request:
	/// GET /file/111.txt
	/// </remarks>
	/// <param name="fileName">File name.</param>
	/// <returns>Returns <see cref="FileContentResult"/>.</returns>
	/// <response code="200">Success.</response>
	/// <response code="404">If file with such name not found on server.</response>
	[HttpGet("{fileName}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult> Download(string fileName)
	{
		string filePath = Path.Combine(_fileStorageDirectory, fileName);
		if (!IOFile.Exists(filePath))
			return NotFound();

		byte[] fileContent = await IOFile.ReadAllBytesAsync(filePath);
		return File(fileContent, "application/octet-stream", fileName);
	}

	/// <summary>
	/// Upload file.
	/// </summary>
	/// <remarks>
	/// Sample request:
	/// POST /file
	/// {
	///     file: "file_content"
	/// }
	/// </remarks>
	/// <param name="uploadFileDto">Uploaded file wrapper.</param>
	/// <returns>Name of uploaded file.</returns>
	/// <response code="200">Success.</response>
	/// <response code="500">If file uploading is failed.</response>
	[HttpPost]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status500InternalServerError)]
	public async Task<ActionResult<string>> Upload([FromForm] UploadFileDto uploadFileDto)
	{
		try
		{
			CreateStorageDirectory();

			IFormFile formFile = uploadFileDto.File;
			string filePath = Path.Combine(_fileStorageDirectory, formFile.FileName);
			await using var fileStream = new FileStream(filePath, FileMode.Create);

			fileStream.Position = 0;
			await formFile.CopyToAsync(fileStream);

			return Ok(formFile.FileName);
		}
		catch
		{
			return StatusCode(
				StatusCodes.Status500InternalServerError,
				$"Не удалось загрузить файл \"{uploadFileDto.File.FileName}\".");
		}
	}


	private void CreateStorageDirectory() => Directory.CreateDirectory(_fileStorageDirectory);
}