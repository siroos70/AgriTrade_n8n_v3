-- AgriTrade v2: عملیات CRUD و فهرست پنل
-- این فایل جدول‌ها را تغییر نمی‌دهد؛ فقط Viewهای خواندنی ایجاد می‌کند.
CREATE OR REPLACE VIEW v_sellers AS
SELECT id AS شناسه,name AS نام,phone AS تلفن,region AS منطقه,status AS وضعیت,created_at AS تاریخ_ثبت FROM sellers;
CREATE OR REPLACE VIEW v_buyers AS
SELECT id AS شناسه,name AS نام,phone AS تلفن,region AS منطقه,status AS وضعیت,created_at AS تاریخ_ثبت FROM buyers;
CREATE OR REPLACE VIEW v_offers AS
SELECT o.id AS شناسه,o.product AS محصول,o.variety AS رقم,o.quantity_kg AS مقدار_کیلو,o.quality AS کیفیت,o.ready_date AS تاریخ_آماده,o.unit_price AS قیمت_هر_کیلو,o.location AS محل,o.status AS وضعیت FROM offers o;
CREATE OR REPLACE VIEW v_demands AS
SELECT d.id AS شناسه,d.product AS محصول,d.variety AS رقم,d.quantity_kg AS مقدار_کیلو,d.min_quality AS حداقل_کیفیت,d.max_unit_price AS حداکثر_قیمت,d.delivery_location AS محل_تحویل,d.needed_date AS تاریخ_نیاز,d.status AS وضعیت FROM demands d;
CREATE OR REPLACE VIEW v_deals AS
SELECT id AS شناسه,product AS محصول,quantity_kg AS مقدار_کیلو,unit_price AS قیمت_هر_کیلو,total_value AS ارزش_کل,status AS وضعیت,agreed_at AS تاریخ_توافق,delivered_at AS تاریخ_تحویل,settled_at AS تاریخ_تسویه FROM deals;
CREATE OR REPLACE VIEW v_commissions AS
SELECT id AS شناسه,deal_id AS شناسه_معامله,commission_type AS نوع,rate AS نرخ,amount AS مبلغ,status AS وضعیت,created_at AS تاریخ_ثبت FROM commissions;
