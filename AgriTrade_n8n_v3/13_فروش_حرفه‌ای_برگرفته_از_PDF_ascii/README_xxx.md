# افزونه فروش حرفه‌ای — استخراج‌شده از PDFها

## تبدیل آموزش به قابلیت عملی
- تعریف بازار هدف → `target_markets` + فرم پنل
- مشخصات مشتری ایده‌آل → `customer_profiles`
- تحلیل رقبا → `competitors` + Workflow ثبت رقیب
- طرح فروش → `sales_plans`
- هدف‌گذاری و برنامه اجرایی → `sales_actions`
- ردیاب سرنخ → `sales_leads` و `lead_activities`
- تیپ ارتباطی مشتری → `customer_personality_assessments`
- درک نیاز → `discovery_questions`
- پیشنهاد ارزش و USP → `value_propositions`
- پیگیری فروش → `sales_followups` و Workflow پیگیری سررسید
- ارائه فروش → `sales_presentations`

## Workflowهای جدید
همه JSONها برای Import در n8n آماده‌اند. پس از Import فقط Credential PostgreSQL را روی Nodeهای PostgreSQL انتخاب کنید. هیچ Token یا Credential داخل بسته ذخیره نشده است.

## محدودیت مهم
این بسته اتصال واقعی به PostgreSQL یا Telegram را بدون Credential محیط شما انجام نمی‌دهد. «قابل Import و اتصال» است؛ فعال‌شدن عملیاتی منوط به تنظیم Credential، HTTPS/CORS و Webhook URL در n8n است.
