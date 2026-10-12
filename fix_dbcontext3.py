import io

path = r"F:\AgriTrade_n8n_v3_PDF_Integrated\backend\AgriTrade.API\Data\ApplicationDbContext.cs"

with io.open(path, "r", encoding="utf-8") as f:
    content = f.read()

content = content.replace("Dbset", "DbSet")

with io.open(path, "w", encoding="utf-8") as f:
    f.write(content)

print("done")