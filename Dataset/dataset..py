from faker import Faker
import random
import json
from datetime import datetime, timedelta

fake = Faker()

# Türkiye'deki şehirler listesi
sehirler = [
    'Adana', 'Adiyaman', 'Afyonkarahisar', 'Agri', 'Amasya', 'Ankara', 'Antalya', 'Artvin', 'Aydin', 'Balikesir',
    'Bilecik', 'Bingol', 'Bitlis', 'Bolu', 'Burdur', 'Bursa', 'Canakkale', 'Cankiri', 'Corum', 'Denizli', 'Diyarbakir',
    'Edirne', 'Elazig', 'Erzincan', 'Erzurum', 'Eskisehir', 'Gaziantep', 'Giresun', 'Gumushane', 'Hakkari', 'Hatay',
    'Isparta', 'Mersin', 'Istanbul', 'Izmir', 'Kars', 'Kastamonu', 'Kayseri', 'Kirklareli', 'Kirsehir', 'Kocaeli',
    'Konya', 'Kutahya', 'Malatya', 'Manisa', 'Kahramanmaras', 'Mardin', 'Mugla', 'Mus', 'Nevsehir', 'Nigde', 'Ordu',
    'Rize', 'Sakarya', 'Samsun', 'Siirt', 'Sinop', 'Sivas', 'Tekirdag', 'Tokat', 'Trabzon', 'Tunceli', 'Sanliurfa',
    'Usak', 'Van', 'Yozgat', 'Zonguldak', 'Aksaray', 'Bayburt', 'Karaman', 'Kirikkale', 'Batman', 'Sirnak', 'Bartin',
    'Ardahan', 'Igdir', 'Yalova', 'Karabuk', 'Kilis', 'Osmaniye', 'Duzce'
]

# Meslekler listesi
meslekler = [
    'Internal Medicine',
    'General Surgery',
    'Plastic Surgery',
    'Otorhinolaryngology',
    'Ophthalmology',
    'Radiology',
    'Cardiology',
    'Psychiatry',
    'Neurology',
    'Orthopedics',
    'Urology',
    'Obstetrics and Gynecology',
    'Pediatrics',
    'Dermatology',
    'Pulmonology',
    'Physical Medicine and Rehabilitation',
    'Forensic Medicine'
]
url =[
    'https://www.cihankaraca.com/wp-content/uploads/2019/08/burunk%C4%B1r%C4%B1klari.jpg',
    'https://synevo.com.tr/upload/b_159201714856488.jpg',
    'https://cf.kizlarsoruyor.com/q10947974/91c91528-8227-4560-b5ca-fd1ec9fe7245.jpg',
    'https://mavimarti.net/wp-content/uploads/2015/08/mmpi-300x200.jpg',
    'https://kbb-forum.net/journal/uploads/figure_KBB_503_1.jpg',
    'https://dentamar.com.tr/_yuk/blogresim/panaromikrontgen.jpg',
    'https://www.webtekno.com/images/editor/default/0003/62/4cc4b6e179797dcbfdee1a70bc7becbc2eb64b4c.jpeg',
    'https://www.hekimoglugoruntuleme.com/wp-content/uploads/2016/01/sinus-grafisi-fiyatlari-2023.jpg',
    'https://cdn.pixabay.com/photo/2016/01/08/22/35/xray-1129430_1280.jpg',
    'https://betatom.com.tr/wp-content/uploads/2023/07/BEYIN1-0-1.jpg',
    'https://betatom.com.tr/wp-content/uploads/2023/07/BEYIN1-2-1.jpg',
    'https://miro.medium.com/v2/resize:fit:1400/1*scU1ilIV95bIqkRX2lDQ8g.jpeg',
    'https://cf.kizlarsoruyor.com/q14643393/primary-share.png?33',
    'https://image.milimaj.com/i/milliyet/75/869x477/5c8e1ead45d2a06b40b2f7cc.jpg',
    'https://www.emartomografi.com/wp-content/uploads/2022/11/kardiyak-mr-fiyatlari.png',
    'https://drgulhizkaratas.com/wp-content/uploads/bobrek-ultrasonu.jpg',
    'https://lh3.googleusercontent.com/proxy/Db8HRixfZsJQ0bx5LDj2xc_M5BvExweMyatf3GCW9wSc3X9XBf9f3B_Q51R5EJGF1iBDhCV8OQM2pju6ALK5zFJgZ4MtsNOHDIWtrqTFDCU7GgM1OYbop32MppRG6NO7j0MkjoP50AncDEVaSKot3lWw0k0K5wY',
    'https://png.pngtree.com/background/20231217/original/pngtree-ultrasound-film-of-a-woman-kidney-kidney-health-ray-photo-picture-image_6861011.jpg',
    'https://www.webtekno.com/images/editor/default/0003/62/6a171940e05345ffa924e22537d347501cdc6687.jpeg'
]
def generate_time_slots():
    times = []
    for hour in range(8, 18):  # 08:00'dan 17:50'ye kadar (doktorların çalışma saatleri varsayılıyor)
        for minute in [0, 10, 20, 30, 40, 50]:
            times.append(f"{hour:02}:{minute:02}")
    return times

# Randevu saatleri oluşturuluyor
available_times = generate_time_slots()

def create_random_appointments(hastalar, doktorlar):
    Randevular = []
    doktor_zamanları = {doktor['ID']: set(available_times) for doktor in doktorlar}

    # Her hasta için en az 100 randevu garantisi
    for hasta in hastalar:
        randevu_sayisi = 0
        while randevu_sayisi < 20:
            randevu_tarihi = fake.date_time_between(start_date="-1y", end_date="now").date()
            doktor = random.choice(doktorlar)
            uygun_saatler = list(doktor_zamanları[doktor['ID']])
            if uygun_saatler:
                randevu_saati = random.choice(uygun_saatler)
                doktor_zamanları[doktor['ID']].remove(randevu_saati)  # Aynı doktora aynı saatte başka randevu vermeyi engellemek
                Randevular.append({
                    'HastaID': hasta['ID'],
                    'DoktorID': doktor['ID'],
                    'RandevuTarihi': randevu_tarihi.strftime('%Y-%m-%d'),
                    'RandevuSaati': randevu_saati
                })
                randevu_sayisi += 1
    
    return Randevular

def create_medical_reports(hastalar, doktorlar):
    raporlar = []
    
    for hasta in hastalar:
        for _ in range(random.randint(5, 15)):
            rapor_tarihi = fake.date_time_between(start_date="-1y", end_date="now")
            doktor = random.choice(doktorlar)
            rapor_url = random.choice(url)  # URL listesinden rastgele bir URL seçiyoruz
            raporlar.append({
                'HastaID': hasta['ID'],
                'DoktorID': doktor['ID'],
                'RaporTarihi': rapor_tarihi.strftime('%Y-%m-%d'),
                'RaporIcerigi': fake.text(),
                'URL': rapor_url
            })
    
    return raporlar


# Hastaları oluşturma
Hastalar = []
for i in range(500):
    first_name = fake.first_name()
    last_name = fake.last_name()
    birth_date = fake.date_of_birth(minimum_age=18, maximum_age=90)
    gender = random.choice(['Female', 'Male'])
    phone_number = fake.random_number(digits=10)
    city = random.choice(sehirler)  # Şehir seçimi
    address = city
    # Hasta ID'si otomatik artan olacak şekilde ekleniyor
    Hastalar.append({
        'ID': i + 1,  # Hasta ID'si otomatik artan olacak şekilde atanıyor
        'Ad': first_name,
        'Soyad': last_name,
        'DogumTarihi': str(birth_date),
        'Cinsiyet': gender,
        'TelefonNumarasi': phone_number,
        'Adres': address,
        'Password': '123456'  # Tüm hastaların şifresi '123456'
    })

# Doktorları oluşturma
Doktorlar = []
for i in range(100):
    first_name = fake.first_name()
    last_name = fake.last_name()
    specialization = random.choice(meslekler)  # Meslekler listesinden rastgele bir meslek seçiyoruz
    hospital_type = random.choice(['City Hospital', 'State Hospital'])
    hospital_city = random.choice(sehirler)  # Şehir seçimi
    hospital = f"{hospital_city} {hospital_type}"
    # Doktor ID'si otomatik artan olacak şekilde ekleniyor
    Doktorlar.append({
        'ID': i + 1,  # Doktor ID'si otomatik artan olacak şekilde atanıyor
        'Ad': first_name,
        'Soyad': last_name,
        'UzmanlikAlani': specialization,
        'CalistigiHastane': hospital,
        'Password': '123456'  # Tüm doktorların şifresi '123456'
    })

# Yönetici oluşturma
Yoneticiler = {'Ad': 'Ferhat', 'Soyad': 'Gocer', 'Password': '123456'}  # Yöneticinin şifresi '123456'

# Hastalar ve doktorlar arasında randevu oluştur
Randevular = create_random_appointments(Hastalar, Doktorlar)

# Tıbbi raporlar oluştur
TibbiRaporlar = create_medical_reports(Hastalar, Doktorlar)

# JSON dosyasına verileri yazma
data = {'Hastalar': Hastalar, 'Doktorlar': Doktorlar, 'Yoneticiler': Yoneticiler, 'Randevular': Randevular, 'TibbiRaporlar': TibbiRaporlar}

with open('2NewDataSet2.json', 'w', encoding='utf-8') as json_file:
    json.dump(data, json_file, ensure_ascii=False, indent=4)
