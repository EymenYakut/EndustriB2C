import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { CampaignDto, CategoryDto, ProductListDto, SliderDto } from '../../core/models/models';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit, OnDestroy {
  categories: CategoryDto[] = [];
  products: ProductListDto[] = [];
  campaigns: CampaignDto[] = [];
  slides: SliderDto[] = [];
  slideIndex = 0;
  orderNumber = '';
  private timer: ReturnType<typeof setInterval> | null = null;
  private paused = false;

  constructor(private api: ApiService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.orderNumber = this.route.snapshot.queryParamMap.get('siparis') || '';
    this.api.products(undefined, undefined, 1, 8).subscribe(p => this.products = p.items || []);
    this.api.campaigns().subscribe(c => this.campaigns = c);
    this.api.categories().subscribe(c => this.categories = c);
    this.api.sliders().subscribe(s => {
      this.slides = s || [];
      this.slideIndex = 0;
      this.startTimer();
    });
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  next(): void {
    if (this.slides.length < 2) return;
    this.slideIndex = (this.slideIndex + 1) % this.slides.length;
    this.startTimer();
  }

  prev(): void {
    if (this.slides.length < 2) return;
    this.slideIndex = (this.slideIndex - 1 + this.slides.length) % this.slides.length;
    this.startTimer();
  }

  goTo(i: number): void {
    this.slideIndex = i;
    this.startTimer();
  }

  isInternal(url?: string): boolean {
    return !!url && url.startsWith('/');
  }

  pad(n: number): string {
    return n < 10 ? '0' + n : String(n);
  }

  pause(): void {
    this.paused = true;
    this.stopTimer();
  }

  resume(): void {
    this.paused = false;
    this.startTimer();
  }

  private startTimer(): void {
    this.stopTimer();
    if (this.paused || this.slides.length < 2) return;
    this.timer = setInterval(() => {
      this.slideIndex = (this.slideIndex + 1) % this.slides.length;
    }, 6000);
  }

  private stopTimer(): void {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }
}
