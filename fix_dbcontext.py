import io

path = r"F:\AgriTrade_n8n_v3_PDF_Integrated\backend\AgriTrade.API\Data\ApplicationDbContext.cs"

with io.open(path, "r", encoding="utf-8") as f:
    content = f.read()

print("RAW BYTES lines 10-18:")
for i, line in enumerate(content.splitlines()[9:18], start=10):
    print(repr(line))

# Fix: replace 'virtual(' with 'virtual Dbset<' patterns and '(' after virtual
# The real issue: 'public virtual(' should be 'public virtual Dbset<...>'
# Let's just normalize: any 'virtual(' -> 'virtual Dbset<' won't work generically.
# Instead, fix the known broken pattern: replace 'virtual(' occurrences.

fixed = content.replace("virtual(", "virtual Dbset<")

# Also fix OnModelBuilder(ModelBuilder(modelBuilder)) -> OnModelBuilder(ModelBuilder builder)
fixed = fixed.replace("OnModelBuilder(ModelBuilder(modelBuilder))", "OnModelBuilder(ModelBuilder builder)")
# Fix references inside: 'modelBuilder' used as variable name but param is 'builder'
# Actually we need to rename usages too. Let's handle by replacing the param name.

# Fix DbOptions< -> Options< ? No, DbOptions is valid in EF.
# The real error CS8124 at (10,46): let's see what's at col 46 after fix.

with io.open(path, "w", encoding="utf-8") as f:
    f.write(fixed)

print("\nAFTER FIX lines 10-18:")
for i, line in enumerate(fixed.splitlines()[9:18], start=10):
    print(repr(line))