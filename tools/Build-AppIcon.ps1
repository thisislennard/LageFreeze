param(
    [string]$SourcePath = (Join-Path $PSScriptRoot "..\src\LageFreeze\Assets\LageFreeze.png"),
    [string]$OutputPath = (Join-Path $PSScriptRoot "..\src\LageFreeze\Assets\LageFreeze.ico")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

$sourceFullPath = [System.IO.Path]::GetFullPath($SourcePath)
$outputFullPath = [System.IO.Path]::GetFullPath($OutputPath)
$sizes = @(16, 20, 24, 32, 40, 48, 64, 128, 256)
$frames = [System.Collections.Generic.List[object]]::new()
$source = [System.Drawing.Bitmap]::FromFile($sourceFullPath)

try {
    foreach ($size in $sizes) {
        $bitmap = [System.Drawing.Bitmap]::new(
            $size,
            $size,
            [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)

        try {
            $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
            try {
                $graphics.Clear([System.Drawing.Color]::Transparent)
                $graphics.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
                $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
                $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
                $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
                $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
                $graphics.DrawImage($source, [System.Drawing.Rectangle]::new(0, 0, $size, $size))
            }
            finally {
                $graphics.Dispose()
            }

            $memory = [System.IO.MemoryStream]::new()
            try {
                $bitmap.Save($memory, [System.Drawing.Imaging.ImageFormat]::Png)
                $frames.Add([pscustomobject]@{
                        Size = $size
                        Data = $memory.ToArray()
                    })
            }
            finally {
                $memory.Dispose()
            }
        }
        finally {
            $bitmap.Dispose()
        }
    }
}
finally {
    $source.Dispose()
}

$outputDirectory = [System.IO.Path]::GetDirectoryName($outputFullPath)
[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
$stream = [System.IO.File]::Create($outputFullPath)
$writer = [System.IO.BinaryWriter]::new($stream)

try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$frames.Count)

    $offset = [uint32](6 + (16 * $frames.Count))
    foreach ($frame in $frames) {
        $dimension = if ($frame.Size -eq 256) { 0 } else { $frame.Size }
        $writer.Write([byte]$dimension)
        $writer.Write([byte]$dimension)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$frame.Data.Length)
        $writer.Write($offset)
        $offset += [uint32]$frame.Data.Length
    }

    foreach ($frame in $frames) {
        $writer.Write([byte[]]$frame.Data)
    }
}
finally {
    $writer.Dispose()
}

Get-Item -LiteralPath $outputFullPath
